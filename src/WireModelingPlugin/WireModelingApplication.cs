using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
using WireModelingPlugin.Commands;

namespace WireModelingPlugin;

/// <summary>
/// Main application class for the Wire Modeling Plugin.
/// Initializes the ribbon UI and registers all commands.
/// </summary>
public class WireModelingApplication : IExternalApplication
{
    public static WireModelingApplication? Instance { get; private set; }

    public Result OnStartup(UIControlledApplication application)
    {
        Instance = this;

        try
        {
            CreateRibbonTab(application);
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Wire Modeling Plugin Error",
                $"Failed to initialize plugin: {ex.Message}");
            return Result.Failed;
        }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    private void CreateRibbonTab(UIControlledApplication application)
    {
        string tabName = "Wire Modeling";
        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Create the ribbon tab
        application.CreateRibbonTab(tabName);

        // Create Draw Panel
        RibbonPanel drawPanel = application.CreateRibbonPanel(tabName, "Draw");

        PushButtonData drawWireButton = new PushButtonData(
            "DrawWire",
            "Draw\nWire",
            assemblyPath,
            typeof(DrawWireCommand).FullName)
        {
            ToolTip = "Draw electrical wire segments point-to-point",
            LongDescription = "Click to define wire path vertices. Creates continuous wire runs with multiple direction changes."
        };
        drawPanel.AddItem(drawWireButton);

        PushButtonData editWireButton = new PushButtonData(
            "EditWire",
            "Edit\nWire",
            assemblyPath,
            typeof(EditWireCommand).FullName)
        {
            ToolTip = "Modify existing wire paths",
            LongDescription = "Select a wire segment to edit its path vertices or properties."
        };
        drawPanel.AddItem(editWireButton);

        // Create Properties Panel
        RibbonPanel propertiesPanel = application.CreateRibbonPanel(tabName, "Properties");

        PushButtonData wirePropertiesButton = new PushButtonData(
            "WireProperties",
            "Wire\nProperties",
            assemblyPath,
            typeof(WirePropertiesCommand).FullName)
        {
            ToolTip = "Set wire specifications for new segments",
            LongDescription = "Configure wire type, gauge, conductor count, and other properties."
        };
        propertiesPanel.AddItem(wirePropertiesButton);

        PushButtonData circuitManagerButton = new PushButtonData(
            "CircuitManager",
            "Circuit\nManager",
            assemblyPath,
            typeof(CircuitManagerCommand).FullName)
        {
            ToolTip = "View and organize circuits and assigned wire",
            LongDescription = "Manage circuit assignments, panel associations, and wire organization."
        };
        propertiesPanel.AddItem(circuitManagerButton);

        // Create Reports Panel
        RibbonPanel reportsPanel = application.CreateRibbonPanel(tabName, "Reports");

        PushButtonData wireScheduleButton = new PushButtonData(
            "WireSchedule",
            "Wire\nSchedule",
            assemblyPath,
            typeof(WireScheduleCommand).FullName)
        {
            ToolTip = "Export formatted wire pull schedule",
            LongDescription = "Generate a wire pull schedule with all wire runs, lengths, and specifications."
        };
        reportsPanel.AddItem(wireScheduleButton);

        PushButtonData materialTakeoffButton = new PushButtonData(
            "MaterialTakeoff",
            "Material\nTakeoff",
            assemblyPath,
            typeof(MaterialTakeoffCommand).FullName)
        {
            ToolTip = "Summarize wire quantities by type and gauge",
            LongDescription = "Generate material takeoff report for procurement and estimation."
        };
        reportsPanel.AddItem(materialTakeoffButton);

        // Create Settings Panel
        RibbonPanel settingsPanel = application.CreateRibbonPanel(tabName, "Settings");

        PushButtonData pluginSettingsButton = new PushButtonData(
            "PluginSettings",
            "Plugin\nSettings",
            assemblyPath,
            typeof(PluginSettingsCommand).FullName)
        {
            ToolTip = "Configure calculation factors and defaults",
            LongDescription = "Set waste percentages, box allowances, panel termination lengths, and other calculation parameters."
        };
        settingsPanel.AddItem(pluginSettingsButton);
    }
}
