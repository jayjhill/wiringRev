using Autodesk.Revit.DB;
using WireModelingPlugin.Models;

namespace WireModelingPlugin.Services;

/// <summary>
/// Service for managing wire data storage and retrieval in Revit.
/// </summary>
public class WireDataService
{
    private readonly Document _document;
    private readonly Dictionary<Guid, WireSegment> _segments = new();
    private readonly Dictionary<Guid, Circuit> _circuits = new();
    private LengthCalculationSettings _calculationSettings = LengthCalculationSettings.Default;

    // Shared parameter GUIDs (would be registered during plugin installation)
    public static readonly Guid WireTypeParamGuid = new("A1B2C3D4-1111-2222-3333-444455556666");
    public static readonly Guid WireGaugeParamGuid = new("A1B2C3D4-1111-2222-3333-444455557777");
    public static readonly Guid ConductorCountParamGuid = new("A1B2C3D4-1111-2222-3333-444455558888");
    public static readonly Guid CircuitNameParamGuid = new("A1B2C3D4-1111-2222-3333-444455559999");
    public static readonly Guid TotalLengthParamGuid = new("A1B2C3D4-1111-2222-3333-44445555AAAA");

    public WireDataService(Document document)
    {
        _document = document;
    }

    /// <summary>
    /// Gets the current calculation settings.
    /// </summary>
    public LengthCalculationSettings CalculationSettings => _calculationSettings;

    /// <summary>
    /// Updates the calculation settings.
    /// </summary>
    public void UpdateCalculationSettings(LengthCalculationSettings settings)
    {
        _calculationSettings = settings;
        RecalculateAllLengths();
    }

    /// <summary>
    /// Adds a new wire segment to the model.
    /// </summary>
    public void AddSegment(WireSegment segment)
    {
        segment.CalculateTotalLength(_calculationSettings);
        _segments[segment.Id] = segment;
    }

    /// <summary>
    /// Gets a wire segment by its ID.
    /// </summary>
    public WireSegment? GetSegment(Guid id)
    {
        return _segments.GetValueOrDefault(id);
    }

    /// <summary>
    /// Gets a wire segment by its Revit element ID.
    /// </summary>
    public WireSegment? GetSegmentByElementId(ElementId elementId)
    {
        return _segments.Values.FirstOrDefault(s => s.RevitElementId == elementId);
    }

    /// <summary>
    /// Gets all wire segments.
    /// </summary>
    public IReadOnlyCollection<WireSegment> GetAllSegments()
    {
        return _segments.Values;
    }

    /// <summary>
    /// Removes a wire segment.
    /// </summary>
    public bool RemoveSegment(Guid id)
    {
        if (_segments.Remove(id, out var segment))
        {
            // Remove from circuit if assigned
            if (segment.CircuitId.HasValue && _circuits.TryGetValue(segment.CircuitId.Value, out var circuit))
            {
                circuit.Segments.RemoveAll(s => s.Id == id);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a new circuit.
    /// </summary>
    public Circuit CreateCircuit(string panelName, string circuitNumber, SystemType systemType = SystemType.Power)
    {
        var circuit = new Circuit
        {
            PanelName = panelName,
            CircuitNumber = circuitNumber,
            Name = $"{panelName}-{circuitNumber}",
            SystemType = systemType
        };

        _circuits[circuit.Id] = circuit;
        return circuit;
    }

    /// <summary>
    /// Gets a circuit by its ID.
    /// </summary>
    public Circuit? GetCircuit(Guid id)
    {
        return _circuits.GetValueOrDefault(id);
    }

    /// <summary>
    /// Gets all circuits.
    /// </summary>
    public IReadOnlyCollection<Circuit> GetAllCircuits()
    {
        return _circuits.Values;
    }

    /// <summary>
    /// Gets circuits filtered by panel name.
    /// </summary>
    public IEnumerable<Circuit> GetCircuitsByPanel(string panelName)
    {
        return _circuits.Values.Where(c =>
            c.PanelName.Equals(panelName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets circuits filtered by system type.
    /// </summary>
    public IEnumerable<Circuit> GetCircuitsBySystemType(SystemType systemType)
    {
        return _circuits.Values.Where(c => c.SystemType == systemType);
    }

    /// <summary>
    /// Assigns a wire segment to a circuit.
    /// </summary>
    public bool AssignSegmentToCircuit(Guid segmentId, Guid circuitId)
    {
        if (!_segments.TryGetValue(segmentId, out var segment))
            return false;

        if (!_circuits.TryGetValue(circuitId, out var circuit))
            return false;

        // Remove from previous circuit if assigned
        if (segment.CircuitId.HasValue && _circuits.TryGetValue(segment.CircuitId.Value, out var oldCircuit))
        {
            oldCircuit.Segments.RemoveAll(s => s.Id == segmentId);
        }

        segment.CircuitId = circuitId;
        circuit.Segments.Add(segment);

        return true;
    }

    /// <summary>
    /// Removes a wire segment from its circuit.
    /// </summary>
    public void UnassignSegmentFromCircuit(Guid segmentId)
    {
        if (!_segments.TryGetValue(segmentId, out var segment))
            return;

        if (segment.CircuitId.HasValue && _circuits.TryGetValue(segment.CircuitId.Value, out var circuit))
        {
            circuit.Segments.RemoveAll(s => s.Id == segmentId);
        }

        segment.CircuitId = null;
    }

    /// <summary>
    /// Recalculates all wire segment lengths.
    /// </summary>
    public void RecalculateAllLengths()
    {
        foreach (var segment in _segments.Values)
        {
            segment.CalculateTotalLength(_calculationSettings);
        }
    }

    /// <summary>
    /// Gets the total wire length for all segments (in feet).
    /// </summary>
    public double GetTotalWireLength()
    {
        return _segments.Values.Sum(s => s.TotalLength);
    }

    /// <summary>
    /// Gets wire length totals grouped by gauge.
    /// </summary>
    public Dictionary<WireGauge, double> GetLengthsByGauge()
    {
        return _segments.Values
            .GroupBy(s => s.Configuration.Gauge)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalLength));
    }

    /// <summary>
    /// Gets wire length totals grouped by wire type.
    /// </summary>
    public Dictionary<string, double> GetLengthsByType()
    {
        return _segments.Values
            .GroupBy(s => s.Configuration.GetSpecificationString())
            .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalLength));
    }

    /// <summary>
    /// Removes a circuit and unassigns all its segments.
    /// </summary>
    public bool RemoveCircuit(Guid circuitId)
    {
        if (!_circuits.Remove(circuitId, out var circuit))
            return false;

        foreach (var segment in circuit.Segments)
        {
            segment.CircuitId = null;
        }

        return true;
    }

    /// <summary>
    /// Gets all panel names in the model.
    /// </summary>
    public IEnumerable<string> GetAllPanelNames()
    {
        return _circuits.Values
            .Select(c => c.PanelName)
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .OrderBy(p => p);
    }

    /// <summary>
    /// Validates all data and returns any issues found.
    /// </summary>
    public List<string> ValidateAllData()
    {
        var issues = new List<string>();

        // Check for unassigned segments
        var unassignedSegments = _segments.Values.Where(s => !s.CircuitId.HasValue).ToList();
        if (unassignedSegments.Count > 0)
        {
            issues.Add($"{unassignedSegments.Count} wire segment(s) are not assigned to a circuit.");
        }

        // Check for disconnected segments
        var disconnectedSegments = _segments.Values.Where(s => !s.HasValidConnections()).ToList();
        if (disconnectedSegments.Count > 0)
        {
            issues.Add($"{disconnectedSegments.Count} wire segment(s) have incomplete connections.");
        }

        // Validate each circuit
        foreach (var circuit in _circuits.Values)
        {
            var result = circuit.Validate();
            issues.AddRange(result.Warnings.Select(w => $"Circuit {circuit.GetStandardName()}: {w}"));
            issues.AddRange(result.Errors.Select(e => $"Circuit {circuit.GetStandardName()} ERROR: {e}"));
        }

        return issues;
    }
}
