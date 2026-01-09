namespace WireModelingPlugin.Models;

/// <summary>
/// Standard AWG (American Wire Gauge) sizes.
/// </summary>
public enum WireGauge
{
    AWG_20,
    AWG_18,
    AWG_16,
    AWG_14,
    AWG_12,
    AWG_10,
    AWG_8,
    AWG_6,
    AWG_4,
    AWG_2,
    AWG_1,
    AWG_1_0,  // 1/0
    AWG_2_0,  // 2/0
    AWG_3_0,  // 3/0
    AWG_4_0   // 4/0
}

/// <summary>
/// Extension methods for WireGauge.
/// </summary>
public static class WireGaugeExtensions
{
    public static string GetDisplayName(this WireGauge gauge) => gauge switch
    {
        WireGauge.AWG_20 => "20 AWG",
        WireGauge.AWG_18 => "18 AWG",
        WireGauge.AWG_16 => "16 AWG",
        WireGauge.AWG_14 => "14 AWG",
        WireGauge.AWG_12 => "12 AWG",
        WireGauge.AWG_10 => "10 AWG",
        WireGauge.AWG_8 => "8 AWG",
        WireGauge.AWG_6 => "6 AWG",
        WireGauge.AWG_4 => "4 AWG",
        WireGauge.AWG_2 => "2 AWG",
        WireGauge.AWG_1 => "1 AWG",
        WireGauge.AWG_1_0 => "1/0 AWG",
        WireGauge.AWG_2_0 => "2/0 AWG",
        WireGauge.AWG_3_0 => "3/0 AWG",
        WireGauge.AWG_4_0 => "4/0 AWG",
        _ => gauge.ToString()
    };

    /// <summary>
    /// Gets the numeric value for calculations (smaller number = larger wire).
    /// </summary>
    public static int GetNumericValue(this WireGauge gauge) => gauge switch
    {
        WireGauge.AWG_20 => 20,
        WireGauge.AWG_18 => 18,
        WireGauge.AWG_16 => 16,
        WireGauge.AWG_14 => 14,
        WireGauge.AWG_12 => 12,
        WireGauge.AWG_10 => 10,
        WireGauge.AWG_8 => 8,
        WireGauge.AWG_6 => 6,
        WireGauge.AWG_4 => 4,
        WireGauge.AWG_2 => 2,
        WireGauge.AWG_1 => 1,
        WireGauge.AWG_1_0 => 0,
        WireGauge.AWG_2_0 => -1,
        WireGauge.AWG_3_0 => -2,
        WireGauge.AWG_4_0 => -3,
        _ => 0
    };

    /// <summary>
    /// Gets the typical ampacity for the wire gauge at 75°C.
    /// </summary>
    public static int GetTypicalAmpacity(this WireGauge gauge) => gauge switch
    {
        WireGauge.AWG_14 => 15,
        WireGauge.AWG_12 => 20,
        WireGauge.AWG_10 => 30,
        WireGauge.AWG_8 => 40,
        WireGauge.AWG_6 => 55,
        WireGauge.AWG_4 => 70,
        WireGauge.AWG_2 => 95,
        WireGauge.AWG_1 => 110,
        WireGauge.AWG_1_0 => 125,
        WireGauge.AWG_2_0 => 145,
        WireGauge.AWG_3_0 => 165,
        WireGauge.AWG_4_0 => 195,
        _ => 0
    };

    /// <summary>
    /// Parses a string to WireGauge.
    /// </summary>
    public static WireGauge? Parse(string value)
    {
        var normalized = value.ToUpperInvariant()
            .Replace(" ", "")
            .Replace("AWG", "")
            .Replace("#", "")
            .Trim();

        return normalized switch
        {
            "20" => WireGauge.AWG_20,
            "18" => WireGauge.AWG_18,
            "16" => WireGauge.AWG_16,
            "14" => WireGauge.AWG_14,
            "12" => WireGauge.AWG_12,
            "10" => WireGauge.AWG_10,
            "8" => WireGauge.AWG_8,
            "6" => WireGauge.AWG_6,
            "4" => WireGauge.AWG_4,
            "2" => WireGauge.AWG_2,
            "1" => WireGauge.AWG_1,
            "1/0" or "0" => WireGauge.AWG_1_0,
            "2/0" or "00" => WireGauge.AWG_2_0,
            "3/0" or "000" => WireGauge.AWG_3_0,
            "4/0" or "0000" => WireGauge.AWG_4_0,
            _ => null
        };
    }
}
