using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WireModelingPlugin.Services;
using WireModelingPlugin.UI;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to generate and export wire pull schedules.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public class WireScheduleCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        try
        {
            var dataService = new WireDataService(doc);

            // Get all wire data from the model
            var circuits = dataService.GetAllCircuits().ToList();
            var unassignedSegments = dataService.GetAllSegments()
                .Where(s => !s.CircuitId.HasValue)
                .ToList();

            if (circuits.Count == 0 && unassignedSegments.Count == 0)
            {
                TaskDialog.Show("Wire Schedule",
                    "No wire data found in the model.\n" +
                    "Draw wire segments and assign them to circuits first.");
                return Result.Cancelled;
            }

            // Show schedule export dialog
            var dialog = new WireScheduleDialog(circuits, unassignedSegments);

            if (dialog.ShowDialog() == true)
            {
                // Export based on selected format
                var exportService = new ExportService();
                string filePath = dialog.SelectedFilePath;

                switch (dialog.SelectedFormat)
                {
                    case ExportFormat.PDF:
                        exportService.ExportToPdf(circuits, filePath);
                        break;
                    case ExportFormat.Excel:
                        exportService.ExportToExcel(circuits, filePath);
                        break;
                    case ExportFormat.CSV:
                        exportService.ExportToCsv(circuits, filePath);
                        break;
                }

                TaskDialog.Show("Wire Schedule",
                    $"Wire schedule exported successfully to:\n{filePath}");

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
