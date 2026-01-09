namespace WireModelingPlugin.Models;

/// <summary>
/// Represents the configuration of a wire type with all its properties.
/// </summary>
public class WireConfiguration
{
    /// <summary>
    /// Unique identifier for this wire configuration.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Display name for this wire configuration (e.g., "12 AWG THHN").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Wire insulation type (THHN, THWN, XHHW, USE, NM-B).
    /// </summary>
    public WireInsulationType InsulationType { get; set; } = WireInsulationType.THHN;

    /// <summary>
    /// Wire gauge (AWG size).
    /// </summary>
    public WireGauge Gauge { get; set; } = WireGauge.AWG_12;

    /// <summary>
    /// Number of current-carrying conductors.
    /// </summary>
    public int ConductorCount { get; set; } = 2;

    /// <summary>
    /// Equipment grounding conductor size.
    /// </summary>
    public WireGauge GroundSize { get; set; } = WireGauge.AWG_12;

    /// <summary>
    /// Neutral conductor size (if different from phase conductors).
    /// </summary>
    public WireGauge? NeutralSize { get; set; }

    /// <summary>
    /// Phase identification color (black, red, blue, etc.).
    /// </summary>
    public string? InsulationColor { get; set; }

    /// <summary>
    /// Voltage rating (300V, 600V, etc.).
    /// </summary>
    public VoltageRating VoltageRating { get; set; } = VoltageRating.V600;

    /// <summary>
    /// Temperature rating of the insulation.
    /// </summary>
    public TemperatureRating TemperatureRating { get; set; } = TemperatureRating.Celsius75;

    /// <summary>
    /// Whether this is a multi-conductor cable (NM-B).
    /// </summary>
    public bool IsMultiConductor => InsulationType == WireInsulationType.NMB;

    /// <summary>
    /// Gets the full specification string (e.g., "3-#12 THHN + #12 GND").
    /// </summary>
    public string GetSpecificationString()
    {
        if (IsMultiConductor)
        {
            return $"{Gauge.GetDisplayName().Replace(" AWG", "")}/{ConductorCount} {InsulationType.GetDisplayName()}";
        }

        string spec = $"{ConductorCount}-#{Gauge.GetDisplayName().Replace(" AWG", "")} {InsulationType.GetDisplayName()}";

        if (GroundSize != Gauge)
        {
            spec += $" + #{GroundSize.GetDisplayName().Replace(" AWG", "")} GND";
        }
        else
        {
            spec += " + GND";
        }

        return spec;
    }

    /// <summary>
    /// Creates a deep copy of this configuration.
    /// </summary>
    public WireConfiguration Clone()
    {
        return new WireConfiguration
        {
            Id = Guid.NewGuid(),
            Name = Name,
            InsulationType = InsulationType,
            Gauge = Gauge,
            ConductorCount = ConductorCount,
            GroundSize = GroundSize,
            NeutralSize = NeutralSize,
            InsulationColor = InsulationColor,
            VoltageRating = VoltageRating,
            TemperatureRating = TemperatureRating
        };
    }

    /// <summary>
    /// Creates standard wire configurations.
    /// </summary>
    public static List<WireConfiguration> GetStandardConfigurations()
    {
        return new List<WireConfiguration>
        {
            // Single conductor configurations
            new()
            {
                Name = "14 AWG THHN (15A Circuit)",
                InsulationType = WireInsulationType.THHN,
                Gauge = WireGauge.AWG_14,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_14
            },
            new()
            {
                Name = "12 AWG THHN (20A Circuit)",
                InsulationType = WireInsulationType.THHN,
                Gauge = WireGauge.AWG_12,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_12
            },
            new()
            {
                Name = "10 AWG THHN (30A Circuit)",
                InsulationType = WireInsulationType.THHN,
                Gauge = WireGauge.AWG_10,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_10
            },
            new()
            {
                Name = "8 AWG THHN (40A Circuit)",
                InsulationType = WireInsulationType.THHN,
                Gauge = WireGauge.AWG_8,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_10
            },
            new()
            {
                Name = "6 AWG THHN (50A Circuit)",
                InsulationType = WireInsulationType.THHN,
                Gauge = WireGauge.AWG_6,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_10
            },
            // NM-B (Romex) configurations
            new()
            {
                Name = "14/2 NM-B",
                InsulationType = WireInsulationType.NMB,
                Gauge = WireGauge.AWG_14,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_14
            },
            new()
            {
                Name = "14/3 NM-B",
                InsulationType = WireInsulationType.NMB,
                Gauge = WireGauge.AWG_14,
                ConductorCount = 3,
                GroundSize = WireGauge.AWG_14
            },
            new()
            {
                Name = "12/2 NM-B",
                InsulationType = WireInsulationType.NMB,
                Gauge = WireGauge.AWG_12,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_12
            },
            new()
            {
                Name = "12/3 NM-B",
                InsulationType = WireInsulationType.NMB,
                Gauge = WireGauge.AWG_12,
                ConductorCount = 3,
                GroundSize = WireGauge.AWG_12
            },
            new()
            {
                Name = "10/2 NM-B",
                InsulationType = WireInsulationType.NMB,
                Gauge = WireGauge.AWG_10,
                ConductorCount = 2,
                GroundSize = WireGauge.AWG_10
            },
            new()
            {
                Name = "10/3 NM-B",
                InsulationType = WireInsulationType.NMB,
                Gauge = WireGauge.AWG_10,
                ConductorCount = 3,
                GroundSize = WireGauge.AWG_10
            }
        };
    }
}
