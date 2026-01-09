using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WireModelingPlugin.Services;
using WireModelingPlugin.UI;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to open the Circuit Manager dialog.
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class CircuitManagerCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        try
        {
            var dataService = new WireDataService(doc);

            // Get panels from the Revit model
            var panels = GetElectricalPanels(doc);

            var dialog = new CircuitManagerDialog(dataService, panels);

            if (dialog.ShowDialog() == true)
            {
                // Apply any changes made in the dialog
                using var transaction = new Transaction(doc, "Update Circuit Assignments");
                transaction.Start();

                // Save changes through the data service
                dataService.RecalculateAllLengths();

                transaction.Commit();

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

    private List<string> GetElectricalPanels(Document doc)
    {
        var panels = new List<string>();

        // Get electrical equipment (panels)
        var collector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
            .WhereElementIsNotElementType();

        foreach (var element in collector)
        {
            var panelName = element.Name;
            if (!string.IsNullOrEmpty(panelName) && !panels.Contains(panelName))
            {
                panels.Add(panelName);
            }

            // Also try to get the Mark parameter
            var mark = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.AsString();
            if (!string.IsNullOrEmpty(mark) && !panels.Contains(mark))
            {
                panels.Add(mark);
            }
        }

        return panels.OrderBy(p => p).ToList();
    }
}
