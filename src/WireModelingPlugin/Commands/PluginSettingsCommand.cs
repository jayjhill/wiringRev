using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WireModelingPlugin.Services;
using WireModelingPlugin.UI;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to open the plugin settings dialog.
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class PluginSettingsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var settings = PluginSettings.Instance;
            var dialog = new PluginSettingsDialog(settings);

            if (dialog.ShowDialog() == true)
            {
                settings.Save();

                TaskDialog.Show("Plugin Settings", "Settings saved successfully.");

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
