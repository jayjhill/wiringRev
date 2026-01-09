namespace WireModelingPlugin.Models;

/// <summary>
/// Settings for wire length calculations including all allowances.
/// </summary>
public class LengthCalculationSettings
{
    /// <summary>
    /// Percentage to add for waste and slack (e.g., 10 = 10%).
    /// </summary>
    public double WastePercentage { get; set; } = 10.0;

    /// <summary>
    /// Fixed length (in feet) to add per junction box.
    /// </summary>
    public double JunctionBoxAllowance { get; set; } = 1.0;

    /// <summary>
    /// Fixed length (in feet) to add for panel terminations.
    /// </summary>
    public double PanelTerminationAllowance { get; set; } = 3.0;

    /// <summary>
    /// Fixed length (in feet) to add for device terminations.
    /// </summary>
    public double DeviceTerminationAllowance { get; set; } = 0.5;

    /// <summary>
    /// Fixed length (in feet) to add for fixture terminations.
    /// </summary>
    public double FixtureTerminationAllowance { get; set; } = 0.5;

    /// <summary>
    /// Whether to round up to nearest foot in final calculations.
    /// </summary>
    public bool RoundUpToNearestFoot { get; set; } = true;

    /// <summary>
    /// Creates default settings with industry-standard values.
    /// </summary>
    public static LengthCalculationSettings Default => new()
    {
        WastePercentage = 10.0,
        JunctionBoxAllowance = 1.0,
        PanelTerminationAllowance = 3.0,
        DeviceTerminationAllowance = 0.5,
        FixtureTerminationAllowance = 0.5,
        RoundUpToNearestFoot = true
    };

    /// <summary>
    /// Creates a copy of these settings.
    /// </summary>
    public LengthCalculationSettings Clone()
    {
        return new LengthCalculationSettings
        {
            WastePercentage = WastePercentage,
            JunctionBoxAllowance = JunctionBoxAllowance,
            PanelTerminationAllowance = PanelTerminationAllowance,
            DeviceTerminationAllowance = DeviceTerminationAllowance,
            FixtureTerminationAllowance = FixtureTerminationAllowance,
            RoundUpToNearestFoot = RoundUpToNearestFoot
        };
    }
}
