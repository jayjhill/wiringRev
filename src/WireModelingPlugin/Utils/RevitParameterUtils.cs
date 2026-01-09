using Autodesk.Revit.DB;

namespace WireModelingPlugin.Utils;

/// <summary>
/// Utility methods for working with Revit parameters.
/// </summary>
public static class RevitParameterUtils
{
    /// <summary>
    /// Creates shared parameters for wire modeling.
    /// </summary>
    public static void EnsureSharedParameters(Document doc)
    {
        // In a production implementation, this would:
        // 1. Check if shared parameters exist
        // 2. Create them if they don't
        // 3. Bind them to appropriate categories

        var app = doc.Application;
        var sharedParamsFile = app.OpenSharedParameterFile();

        if (sharedParamsFile == null)
        {
            // Would need to create or prompt for shared parameter file
            return;
        }

        // Define parameter names and their properties
        var parameters = new Dictionary<string, (SpecTypeId SpecType, bool IsInstance)>
        {
            { "Wire Type", (SpecTypeId.String.Text, true) },
            { "Wire Gauge", (SpecTypeId.String.Text, true) },
            { "Conductor Count", (SpecTypeId.Int.Integer, true) },
            { "Ground Size", (SpecTypeId.String.Text, true) },
            { "Voltage Rating", (SpecTypeId.String.Text, true) },
            { "Temperature Rating", (SpecTypeId.String.Text, true) },
            { "Wire Specification", (SpecTypeId.String.Text, true) },
            { "Wire Length", (SpecTypeId.Length, true) },
            { "Circuit Name", (SpecTypeId.String.Text, true) },
            { "Panel Name", (SpecTypeId.String.Text, true) }
        };

        // Check if our group exists, create if not
        var groupName = "Wire Modeling Plugin";
        DefinitionGroup? group = null;

        foreach (DefinitionGroup g in sharedParamsFile.Groups)
        {
            if (g.Name == groupName)
            {
                group = g;
                break;
            }
        }

        if (group == null)
        {
            group = sharedParamsFile.Groups.Create(groupName);
        }

        // Create parameters that don't exist
        foreach (var (paramName, (specType, isInstance)) in parameters)
        {
            if (group.Definitions.get_Item(paramName) == null)
            {
                var options = new ExternalDefinitionCreationOptions(paramName, specType)
                {
                    Visible = true,
                    UserModifiable = true
                };

                group.Definitions.Create(options);
            }
        }

        // Bind parameters to Generic Model category (for DirectShape wire elements)
        BindParametersToCategory(doc, group, BuiltInCategory.OST_GenericModel, parameters);
    }

    private static void BindParametersToCategory(
        Document doc,
        DefinitionGroup group,
        BuiltInCategory category,
        Dictionary<string, (SpecTypeId SpecType, bool IsInstance)> parameters)
    {
        var categorySet = new CategorySet();
        var cat = doc.Settings.Categories.get_Item(category);
        if (cat != null)
        {
            categorySet.Insert(cat);
        }

        using var transaction = new Transaction(doc, "Bind Wire Parameters");
        transaction.Start();

        var bindingMap = doc.ParameterBindings;

        foreach (var (paramName, (specType, isInstance)) in parameters)
        {
            var definition = group.Definitions.get_Item(paramName);
            if (definition == null)
                continue;

            // Check if already bound
            var existingBinding = bindingMap.get_Item(definition);
            if (existingBinding != null)
                continue;

            // Create binding
            Binding binding = isInstance
                ? (Binding)doc.Application.Create.NewInstanceBinding(categorySet)
                : (Binding)doc.Application.Create.NewTypeBinding(categorySet);

            bindingMap.Insert(definition, binding);
        }

        transaction.Commit();
    }

    /// <summary>
    /// Gets a parameter value as a string.
    /// </summary>
    public static string? GetParameterAsString(Element element, string paramName)
    {
        var param = element.LookupParameter(paramName);
        return param?.AsString();
    }

    /// <summary>
    /// Gets a parameter value as an integer.
    /// </summary>
    public static int? GetParameterAsInt(Element element, string paramName)
    {
        var param = element.LookupParameter(paramName);
        return param?.AsInteger();
    }

    /// <summary>
    /// Gets a parameter value as a double.
    /// </summary>
    public static double? GetParameterAsDouble(Element element, string paramName)
    {
        var param = element.LookupParameter(paramName);
        return param?.AsDouble();
    }

    /// <summary>
    /// Sets a parameter value.
    /// </summary>
    public static bool SetParameter(Element element, string paramName, object value)
    {
        var param = element.LookupParameter(paramName);
        if (param == null || param.IsReadOnly)
            return false;

        switch (value)
        {
            case string s:
                param.Set(s);
                return true;
            case int i:
                param.Set(i);
                return true;
            case double d:
                param.Set(d);
                return true;
            case ElementId id:
                param.Set(id);
                return true;
            default:
                return false;
        }
    }
}
