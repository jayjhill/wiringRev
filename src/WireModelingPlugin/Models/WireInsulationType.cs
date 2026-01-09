namespace WireModelingPlugin.Models;

/// <summary>
/// Wire insulation types per NEC standards.
/// </summary>
public enum WireInsulationType
{
    /// <summary>
    /// Thermoplastic High Heat-resistant Nylon-coated
    /// </summary>
    THHN,

    /// <summary>
    /// Thermoplastic Heat and Water-resistant Nylon-coated
    /// </summary>
    THWN,

    /// <summary>
    /// Cross-linked polyethylene High Heat-resistant Water-resistant
    /// </summary>
    XHHW,

    /// <summary>
    /// Underground Service Entrance
    /// </summary>
    USE,

    /// <summary>
    /// Non-Metallic Sheathed Cable (Romex)
    /// </summary>
    NMB
}

/// <summary>
/// Extension methods for WireInsulationType.
/// </summary>
public static class WireInsulationTypeExtensions
{
    public static string GetDisplayName(this WireInsulationType type) => type switch
    {
        WireInsulationType.THHN => "THHN",
        WireInsulationType.THWN => "THWN",
        WireInsulationType.XHHW => "XHHW",
        WireInsulationType.USE => "USE",
        WireInsulationType.NMB => "NM-B",
        _ => type.ToString()
    };

    public static string GetDescription(this WireInsulationType type) => type switch
    {
        WireInsulationType.THHN => "Thermoplastic High Heat-resistant Nylon-coated",
        WireInsulationType.THWN => "Thermoplastic Heat and Water-resistant Nylon-coated",
        WireInsulationType.XHHW => "Cross-linked polyethylene High Heat-resistant Water-resistant",
        WireInsulationType.USE => "Underground Service Entrance",
        WireInsulationType.NMB => "Non-Metallic Sheathed Cable (Romex)",
        _ => string.Empty
    };
}
