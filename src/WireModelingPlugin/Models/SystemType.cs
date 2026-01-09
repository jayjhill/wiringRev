namespace WireModelingPlugin.Models;

/// <summary>
/// Electrical system categorization types.
/// </summary>
public enum SystemType
{
    /// <summary>
    /// General lighting circuits
    /// </summary>
    Lighting,

    /// <summary>
    /// Receptacle/outlet circuits
    /// </summary>
    Receptacle,

    /// <summary>
    /// Mechanical equipment circuits (HVAC, motors)
    /// </summary>
    Mechanical,

    /// <summary>
    /// Fire alarm system wiring
    /// </summary>
    FireAlarm,

    /// <summary>
    /// Low voltage systems (data, telecom, security)
    /// </summary>
    LowVoltage,

    /// <summary>
    /// Power distribution circuits
    /// </summary>
    Power
}

public static class SystemTypeExtensions
{
    public static string GetDisplayName(this SystemType type) => type switch
    {
        SystemType.Lighting => "Lighting",
        SystemType.Receptacle => "Receptacle",
        SystemType.Mechanical => "Mechanical",
        SystemType.FireAlarm => "Fire Alarm",
        SystemType.LowVoltage => "Low Voltage",
        SystemType.Power => "Power",
        _ => type.ToString()
    };
}
