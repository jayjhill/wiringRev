namespace WireModelingPlugin.Models;

/// <summary>
/// Wire insulation temperature ratings.
/// </summary>
public enum TemperatureRating
{
    /// <summary>
    /// 60°C rated insulation
    /// </summary>
    Celsius60,

    /// <summary>
    /// 75°C rated insulation
    /// </summary>
    Celsius75,

    /// <summary>
    /// 90°C rated insulation
    /// </summary>
    Celsius90
}

public static class TemperatureRatingExtensions
{
    public static string GetDisplayName(this TemperatureRating rating) => rating switch
    {
        TemperatureRating.Celsius60 => "60°C",
        TemperatureRating.Celsius75 => "75°C",
        TemperatureRating.Celsius90 => "90°C",
        _ => rating.ToString()
    };

    public static int GetCelsiusValue(this TemperatureRating rating) => rating switch
    {
        TemperatureRating.Celsius60 => 60,
        TemperatureRating.Celsius75 => 75,
        TemperatureRating.Celsius90 => 90,
        _ => 75
    };
}
