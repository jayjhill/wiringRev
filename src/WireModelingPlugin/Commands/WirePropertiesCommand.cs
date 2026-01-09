using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WireModelingPlugin.Services;
using WireModelingPlugin.UI;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to set default wire properties for new segments.
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class WirePropertiesCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var settings = PluginSettings.Instance;
            var currentConfig = settings.DefaultWireConfiguration.Clone();

            var dialog = new WirePropertiesDialog(currentConfig, isDefaultSettings: true);

            if (dialog.ShowDialog() == true)
            {
                settings.DefaultWireConfiguration = dialog.Configuration;
                settings.Save();

                TaskDialog.Show("Wire Properties",
                    $"Default wire properties updated:\n" +
                    $"Type: {dialog.Configuration.GetSpecificationString()}");

                return Result.Succeeded;
            }

            return Result.Cancelled;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}
