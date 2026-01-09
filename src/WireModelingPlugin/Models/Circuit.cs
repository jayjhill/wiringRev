using Autodesk.Revit.DB;

namespace WireModelingPlugin.Models;

/// <summary>
/// Represents an electrical circuit containing wire segments.
/// </summary>
public class Circuit
{
    /// <summary>
    /// Unique identifier for this circuit.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The Revit electrical circuit element ID (if linked to native circuit).
    /// </summary>
    public ElementId? RevitCircuitId { get; set; }

    /// <summary>
    /// Circuit name following standard convention (Panel-Circuit#).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The distribution panel this circuit is fed from.
    /// </summary>
    public string PanelName { get; set; } = string.Empty;

    /// <summary>
    /// The circuit number within the panel.
    /// </summary>
    public string CircuitNumber { get; set; } = string.Empty;

    /// <summary>
    /// The system type categorization.
    /// </summary>
    public SystemType SystemType { get; set; } = SystemType.Power;

    /// <summary>
    /// Circuit description or purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Wire segments that belong to this circuit.
    /// </summary>
    public List<WireSegment> Segments { get; set; } = new();

    /// <summary>
    /// Whether this is a multi-wire branch circuit (shared neutral).
    /// </summary>
    public bool IsMultiWireBranchCircuit { get; set; }

    /// <summary>
    /// For MWBC, the paired circuit number.
    /// </summary>
    public string? PairedCircuitNumber { get; set; }

    /// <summary>
    /// The breaker/overcurrent device size in amps.
    /// </summary>
    public int BreakerSize { get; set; } = 20;

    /// <summary>
    /// Number of poles for the breaker.
    /// </summary>
    public int Poles { get; set; } = 1;

    /// <summary>
    /// Gets the total raw length of all wire segments in the circuit.
    /// </summary>
    public double GetTotalRawLength()
    {
        return Segments.Sum(s => s.RawLength);
    }

    /// <summary>
    /// Gets the total calculated length (with allowances) of all segments.
    /// </summary>
    public double GetTotalLength()
    {
        return Segments.Sum(s => s.TotalLength);
    }

    /// <summary>
    /// Gets the standard circuit name (Panel-Circuit#).
    /// </summary>
    public string GetStandardName()
    {
        if (!string.IsNullOrEmpty(Name))
            return Name;

        if (!string.IsNullOrEmpty(PanelName) && !string.IsNullOrEmpty(CircuitNumber))
            return $"{PanelName}-{CircuitNumber}";

        return $"Circuit-{Id.ToString()[..8]}";
    }

    /// <summary>
    /// Recalculates all segment lengths using the provided settings.
    /// </summary>
    public void RecalculateLengths(LengthCalculationSettings settings)
    {
        foreach (var segment in Segments)
        {
            segment.CalculateTotalLength(settings);
        }
    }

    /// <summary>
    /// Gets the primary wire configuration for this circuit.
    /// </summary>
    public WireConfiguration? GetPrimaryConfiguration()
    {
        return Segments.FirstOrDefault()?.Configuration;
    }

    /// <summary>
    /// Validates the circuit for completeness.
    /// </summary>
    public CircuitValidationResult Validate()
    {
        var result = new CircuitValidationResult { IsValid = true };

        if (string.IsNullOrEmpty(PanelName))
        {
            result.IsValid = false;
            result.Warnings.Add("Circuit is not assigned to a panel.");
        }

        if (string.IsNullOrEmpty(CircuitNumber))
        {
            result.IsValid = false;
            result.Warnings.Add("Circuit number is not specified.");
        }

        if (Segments.Count == 0)
        {
            result.Warnings.Add("Circuit has no wire segments.");
        }

        var disconnectedSegments = Segments.Where(s => !s.HasValidConnections()).ToList();
        if (disconnectedSegments.Count > 0)
        {
            result.Warnings.Add($"{disconnectedSegments.Count} segment(s) are not fully connected.");
        }

        var unverifiedSegments = Segments.Where(s => !s.IsVerified).ToList();
        if (unverifiedSegments.Count > 0)
        {
            result.Warnings.Add($"{unverifiedSegments.Count} segment(s) are not verified.");
        }

        return result;
    }
}

/// <summary>
/// Result of circuit validation.
/// </summary>
public class CircuitValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
