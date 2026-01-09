using System.Text.Json;
using WireModelingPlugin.Models;

namespace WireModelingPlugin.Services;

/// <summary>
/// Plugin-wide settings that persist across sessions.
/// </summary>
public class PluginSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WireModelingPlugin",
        "settings.json");

    private static PluginSettings? _instance;

    /// <summary>
    /// Gets the singleton instance of plugin settings.
    /// </summary>
    public static PluginSettings Instance => _instance ??= Load();

    /// <summary>
    /// Length calculation settings.
    /// </summary>
    public LengthCalculationSettings LengthCalculation { get; set; } = LengthCalculationSettings.Default;

    /// <summary>
    /// Default wire configuration for new segments.
    /// </summary>
    public WireConfiguration DefaultWireConfiguration { get; set; } = new()
    {
        Name = "12 AWG THHN (20A Circuit)",
        InsulationType = WireInsulationType.THHN,
        Gauge = WireGauge.AWG_12,
        ConductorCount = 2,
        GroundSize = WireGauge.AWG_12
    };

    /// <summary>
    /// Default length unit for display.
    /// </summary>
    public LengthUnit DefaultLengthUnit { get; set; } = LengthUnit.Feet;

    /// <summary>
    /// Whether to show real-time preview while drawing.
    /// </summary>
    public bool ShowDrawingPreview { get; set; } = true;

    /// <summary>
    /// Whether to auto-snap to electrical connectors.
    /// </summary>
    public bool AutoSnapToConnectors { get; set; } = true;

    /// <summary>
    /// Snap tolerance in feet.
    /// </summary>
    public double SnapTolerance { get; set; } = 0.5;

    /// <summary>
    /// Whether to automatically assign new segments to circuits.
    /// </summary>
    public bool AutoAssignToCircuit { get; set; } = true;

    /// <summary>
    /// Default export format for schedules.
    /// </summary>
    public ExportFormat DefaultExportFormat { get; set; } = ExportFormat.Excel;

    /// <summary>
    /// Recently used wire configurations (for quick selection).
    /// </summary>
    public List<WireConfiguration> RecentConfigurations { get; set; } = new();

    /// <summary>
    /// Maximum number of recent configurations to keep.
    /// </summary>
    public int MaxRecentConfigurations { get; set; } = 10;

    /// <summary>
    /// Loads settings from disk or creates defaults.
    /// </summary>
    public static PluginSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<PluginSettings>(json);
                if (settings != null)
                    return settings;
            }
        }
        catch
        {
            // If loading fails, return defaults
        }

        return new PluginSettings();
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail on save errors
        }
    }

    /// <summary>
    /// Adds a configuration to recent list.
    /// </summary>
    public void AddRecentConfiguration(WireConfiguration config)
    {
        // Remove if already exists
        RecentConfigurations.RemoveAll(c => c.Id == config.Id);

        // Add to beginning
        RecentConfigurations.Insert(0, config);

        // Trim to max size
        while (RecentConfigurations.Count > MaxRecentConfigurations)
        {
            RecentConfigurations.RemoveAt(RecentConfigurations.Count - 1);
        }

        Save();
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        LengthCalculation = LengthCalculationSettings.Default;
        DefaultWireConfiguration = new WireConfiguration
        {
            Name = "12 AWG THHN (20A Circuit)",
            InsulationType = WireInsulationType.THHN,
            Gauge = WireGauge.AWG_12,
            ConductorCount = 2,
            GroundSize = WireGauge.AWG_12
        };
        DefaultLengthUnit = LengthUnit.Feet;
        ShowDrawingPreview = true;
        AutoSnapToConnectors = true;
        SnapTolerance = 0.5;
        AutoAssignToCircuit = true;
        DefaultExportFormat = ExportFormat.Excel;
        RecentConfigurations.Clear();

        Save();
    }
}

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    PDF,
    Excel,
    CSV
}
