using System.Windows.Media;
using WireModelingPlugin.Models;

namespace WireModelingPlugin.Services;

/// <summary>
/// Service for managing wire display colors based on gauge and configuration.
/// </summary>
public static class WireColorService
{
    /// <summary>
    /// Gets the display color for a single conductor wire based on gauge.
    /// </summary>
    public static Color GetSingleConductorColor(WireGauge gauge)
    {
        return gauge switch
        {
            WireGauge.AWG_14 => Colors.White,
            WireGauge.AWG_12 => Colors.Yellow,
            WireGauge.AWG_10 => Colors.Orange,
            WireGauge.AWG_8 or WireGauge.AWG_6 => Colors.Black,
            _ => Colors.Gray // Default for other sizes
        };
    }

    /// <summary>
    /// Gets the display color for NM-B multi-conductor cable based on configuration.
    /// </summary>
    public static Color GetMultiConductorColor(WireGauge gauge, int conductorCount)
    {
        // 3-conductor configurations
        if (conductorCount == 3)
        {
            return gauge switch
            {
                WireGauge.AWG_14 => Colors.Blue,    // 14/3 NM-B
                WireGauge.AWG_12 => Colors.Purple,  // 12/3 NM-B
                WireGauge.AWG_10 => Colors.Pink,    // 10/3 NM-B
                _ => Colors.DarkGray
            };
        }

        // 2-conductor configurations (use single conductor colors)
        return GetSingleConductorColor(gauge);
    }

    /// <summary>
    /// Gets the appropriate display color for a wire configuration.
    /// </summary>
    public static Color GetDisplayColor(WireConfiguration config)
    {
        if (config.IsMultiConductor)
        {
            return GetMultiConductorColor(config.Gauge, config.ConductorCount);
        }

        return GetSingleConductorColor(config.Gauge);
    }

    /// <summary>
    /// Gets the Revit color (as RGB integers) for a wire configuration.
    /// </summary>
    public static (byte R, byte G, byte B) GetRevitColor(WireConfiguration config)
    {
        var color = GetDisplayColor(config);
        return (color.R, color.G, color.B);
    }

    /// <summary>
    /// Gets a human-readable color name for display.
    /// </summary>
    public static string GetColorName(WireConfiguration config)
    {
        var color = GetDisplayColor(config);

        return color switch
        {
            var c when c == Colors.White => "White",
            var c when c == Colors.Yellow => "Yellow",
            var c when c == Colors.Orange => "Orange",
            var c when c == Colors.Black => "Black",
            var c when c == Colors.Blue => "Blue",
            var c when c == Colors.Purple => "Purple",
            var c when c == Colors.Pink => "Pink",
            var c when c == Colors.Gray => "Gray",
            var c when c == Colors.DarkGray => "Dark Gray",
            _ => $"RGB({color.R},{color.G},{color.B})"
        };
    }

    /// <summary>
    /// Gets the standard insulation color description for a phase conductor.
    /// </summary>
    public static string GetPhaseColorDescription(int phaseNumber, int voltageSystem = 120)
    {
        // Standard US phase colors for 120/208V or 120/240V systems
        return phaseNumber switch
        {
            1 => "Black (Phase A)",
            2 => "Red (Phase B)",
            3 => "Blue (Phase C)",
            _ => "Black"
        };
    }

    /// <summary>
    /// Gets all available display colors with their descriptions.
    /// </summary>
    public static Dictionary<string, Color> GetAllDisplayColors()
    {
        return new Dictionary<string, Color>
        {
            { "White (14 AWG)", Colors.White },
            { "Yellow (12 AWG)", Colors.Yellow },
            { "Orange (10 AWG)", Colors.Orange },
            { "Black (8/6 AWG)", Colors.Black },
            { "Blue (14/3 NM-B)", Colors.Blue },
            { "Purple (12/3 NM-B)", Colors.Purple },
            { "Pink (10/3 NM-B)", Colors.Pink },
            { "Gray (Other)", Colors.Gray }
        };
    }
}
