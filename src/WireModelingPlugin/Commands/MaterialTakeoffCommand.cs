using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WireModelingPlugin.Services;
using WireModelingPlugin.UI;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to generate material takeoff reports.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public class MaterialTakeoffCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        try
        {
            var dataService = new WireDataService(doc);

            // Get wire quantities
            var lengthsByType = dataService.GetLengthsByType();
            var lengthsByGauge = dataService.GetLengthsByGauge();
            var totalLength = dataService.GetTotalWireLength();

            if (totalLength == 0)
            {
                TaskDialog.Show("Material Takeoff",
                    "No wire data found in the model.\n" +
                    "Draw wire segments first.");
                return Result.Cancelled;
            }

            // Show material takeoff dialog
            var dialog = new MaterialTakeoffDialog(lengthsByType, lengthsByGauge, totalLength);

            if (dialog.ShowDialog() == true)
            {
                // Export based on selected format
                var exportService = new ExportService();
                string filePath = dialog.SelectedFilePath;

                switch (dialog.SelectedFormat)
                {
                    case ExportFormat.PDF:
                        exportService.ExportMaterialTakeoffToPdf(lengthsByType, lengthsByGauge, filePath);
                        break;
                    case ExportFormat.Excel:
                        exportService.ExportMaterialTakeoffToExcel(lengthsByType, lengthsByGauge, filePath);
                        break;
                    case ExportFormat.CSV:
                        exportService.ExportMaterialTakeoffToCsv(lengthsByType, lengthsByGauge, filePath);
                        break;
                }

                TaskDialog.Show("Material Takeoff",
                    $"Material takeoff exported successfully to:\n{filePath}");

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
