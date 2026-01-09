namespace WireModelingPlugin.Models;

/// <summary>
/// Standard voltage ratings for electrical wire.
/// </summary>
public enum VoltageRating
{
    /// <summary>
    /// 300 volt rated
    /// </summary>
    V300 = 300,

    /// <summary>
    /// 600 volt rated
    /// </summary>
    V600 = 600
}

public static class VoltageRatingExtensions
{
    public static string GetDisplayName(this VoltageRating rating) => rating switch
    {
        VoltageRating.V300 => "300V",
        VoltageRating.V600 => "600V",
        _ => $"{(int)rating}V"
    };
}
