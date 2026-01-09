using Autodesk.Revit.DB;

namespace WireModelingPlugin.Models;

/// <summary>
/// Represents a single wire segment with its path and properties.
/// </summary>
public class WireSegment
{
    /// <summary>
    /// The Revit element ID of this wire segment.
    /// </summary>
    public ElementId? RevitElementId { get; set; }

    /// <summary>
    /// Unique identifier for this segment.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The wire configuration for this segment.
    /// </summary>
    public WireConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// The path vertices defining this wire segment in 3D space.
    /// Points are stored in Revit internal units (feet).
    /// </summary>
    public List<XYZ> PathPoints { get; set; } = new();

    /// <summary>
    /// The circuit this wire segment belongs to.
    /// </summary>
    public Guid? CircuitId { get; set; }

    /// <summary>
    /// The start connector element ID (panel, device, fixture).
    /// </summary>
    public ElementId? StartConnectorId { get; set; }

    /// <summary>
    /// The end connector element ID (panel, device, fixture).
    /// </summary>
    public ElementId? EndConnectorId { get; set; }

    /// <summary>
    /// Calculated segment length in feet (before allowances).
    /// </summary>
    public double RawLength { get; private set; }

    /// <summary>
    /// Calculated total length including allowances (in feet).
    /// </summary>
    public double TotalLength { get; private set; }

    /// <summary>
    /// Additional notes or comments for this segment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this segment has been verified/checked.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Calculates the raw length from path points.
    /// </summary>
    public void CalculateRawLength()
    {
        RawLength = 0;

        if (PathPoints.Count < 2)
            return;

        for (int i = 0; i < PathPoints.Count - 1; i++)
        {
            RawLength += PathPoints[i].DistanceTo(PathPoints[i + 1]);
        }
    }

    /// <summary>
    /// Calculates the total length including allowances.
    /// </summary>
    /// <param name="settings">The calculation settings to apply.</param>
    public void CalculateTotalLength(LengthCalculationSettings settings)
    {
        CalculateRawLength();

        double length = RawLength;

        // Apply waste/slack percentage
        length *= (1 + settings.WastePercentage / 100.0);

        // Add box allowances (count junction boxes in path)
        int junctionBoxCount = Math.Max(0, PathPoints.Count - 2);
        length += junctionBoxCount * settings.JunctionBoxAllowance;

        // Add panel termination allowance if connected to panel
        if (StartConnectorId != null)
        {
            length += settings.PanelTerminationAllowance;
        }

        // Add device termination allowance if connected to device
        if (EndConnectorId != null)
        {
            length += settings.DeviceTerminationAllowance;
        }

        TotalLength = length;
    }

    /// <summary>
    /// Gets the length in the specified unit.
    /// </summary>
    public double GetLengthInUnit(LengthUnit unit)
    {
        return unit switch
        {
            LengthUnit.Feet => TotalLength,
            LengthUnit.Inches => TotalLength * 12,
            LengthUnit.Meters => TotalLength * 0.3048,
            _ => TotalLength
        };
    }

    /// <summary>
    /// Checks if the segment has valid connections at both ends.
    /// </summary>
    public bool HasValidConnections()
    {
        return StartConnectorId != null && EndConnectorId != null;
    }

    /// <summary>
    /// Gets the bounding box of this wire segment.
    /// </summary>
    public BoundingBoxXYZ? GetBoundingBox()
    {
        if (PathPoints.Count == 0)
            return null;

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        foreach (var point in PathPoints)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            minZ = Math.Min(minZ, point.Z);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
            maxZ = Math.Max(maxZ, point.Z);
        }

        return new BoundingBoxXYZ
        {
            Min = new XYZ(minX, minY, minZ),
            Max = new XYZ(maxX, maxY, maxZ)
        };
    }
}

/// <summary>
/// Length units for display and export.
/// </summary>
public enum LengthUnit
{
    Feet,
    Inches,
    Meters
}
