using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;
using WireModelingPlugin.UI;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to edit existing wire segments.
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class EditWireCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        try
        {
            // Prompt user to select a wire element
            TaskDialog.Show("Edit Wire", "Select a wire segment to edit.");

            var selection = uiDoc.Selection;
            var reference = selection.PickObject(
                ObjectType.Element,
                new WireSelectionFilter(),
                "Select a wire segment to edit");

            if (reference == null)
            {
                return Result.Cancelled;
            }

            var element = doc.GetElement(reference.ElementId);
            if (element == null)
            {
                TaskDialog.Show("Edit Wire", "Could not find the selected element.");
                return Result.Failed;
            }

            // Get wire data from element
            var wireConfig = GetWireConfiguration(element);

            // Show edit dialog
            var editDialog = new WireEditDialog(element, wireConfig);
            if (editDialog.ShowDialog() != true)
            {
                return Result.Cancelled;
            }

            // Apply changes
            using var transaction = new Transaction(doc, "Edit Wire Segment");
            transaction.Start();

            // Update wire properties
            UpdateWireProperties(element, editDialog.Configuration);

            // Update color override
            UpdateWireColor(doc, uiDoc.ActiveView, element, editDialog.Configuration);

            transaction.Commit();

            TaskDialog.Show("Edit Wire",
                $"Wire segment updated:\n" +
                $"Type: {editDialog.Configuration.GetSpecificationString()}");

            return Result.Succeeded;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }

    private WireConfiguration GetWireConfiguration(Element element)
    {
        var config = new WireConfiguration();

        // Try to read existing parameters
        var wireType = element.LookupParameter("Wire Type")?.AsString();
        if (!string.IsNullOrEmpty(wireType))
        {
            config.InsulationType = wireType.ToUpperInvariant() switch
            {
                "THHN" => WireInsulationType.THHN,
                "THWN" => WireInsulationType.THWN,
                "XHHW" => WireInsulationType.XHHW,
                "USE" => WireInsulationType.USE,
                "NM-B" or "NMB" => WireInsulationType.NMB,
                _ => WireInsulationType.THHN
            };
        }

        var gaugeStr = element.LookupParameter("Wire Gauge")?.AsString();
        if (!string.IsNullOrEmpty(gaugeStr))
        {
            var gauge = WireGaugeExtensions.Parse(gaugeStr);
            if (gauge.HasValue)
            {
                config.Gauge = gauge.Value;
            }
        }

        var conductorCount = element.LookupParameter("Conductor Count")?.AsInteger();
        if (conductorCount.HasValue && conductorCount.Value > 0)
        {
            config.ConductorCount = conductorCount.Value;
        }

        var groundStr = element.LookupParameter("Ground Size")?.AsString();
        if (!string.IsNullOrEmpty(groundStr))
        {
            var ground = WireGaugeExtensions.Parse(groundStr);
            if (ground.HasValue)
            {
                config.GroundSize = ground.Value;
            }
        }

        return config;
    }

    private void UpdateWireProperties(Element element, WireConfiguration config)
    {
        SetParameter(element, "Wire Type", config.InsulationType.GetDisplayName());
        SetParameter(element, "Wire Gauge", config.Gauge.GetDisplayName());
        SetParameter(element, "Conductor Count", config.ConductorCount);
        SetParameter(element, "Ground Size", config.GroundSize.GetDisplayName());
        SetParameter(element, "Voltage Rating", config.VoltageRating.GetDisplayName());
        SetParameter(element, "Temperature Rating", config.TemperatureRating.GetDisplayName());
        SetParameter(element, "Wire Specification", config.GetSpecificationString());
    }

    private void UpdateWireColor(Document doc, View view, Element element, WireConfiguration config)
    {
        var (r, g, b) = WireColorService.GetRevitColor(config);
        var overrideSettings = new OverrideGraphicSettings();
        overrideSettings.SetProjectionLineColor(new Color(r, g, b));
        overrideSettings.SetSurfaceForegroundPatternColor(new Color(r, g, b));
        view.SetElementOverrides(element.Id, overrideSettings);
    }

    private void SetParameter(Element element, string paramName, object value)
    {
        var param = element.LookupParameter(paramName);
        if (param != null && !param.IsReadOnly)
        {
            switch (value)
            {
                case string s:
                    param.Set(s);
                    break;
                case int i:
                    param.Set(i);
                    break;
                case double d:
                    param.Set(d);
                    break;
            }
        }
    }
}

/// <summary>
/// Selection filter for wire elements.
/// </summary>
public class WireSelectionFilter : ISelectionFilter
{
    public bool AllowElement(Element elem)
    {
        // Allow DirectShapes with wire data or model curves representing wire
        if (elem is DirectShape ds)
        {
            return ds.Name?.StartsWith("Wire-") == true;
        }

        // Also allow model curves that might be wire
        if (elem is ModelCurve)
        {
            var wireType = elem.LookupParameter("Wire Type");
            return wireType != null;
        }

        return false;
    }

    public bool AllowReference(Reference reference, XYZ position)
    {
        return true;
    }
}
