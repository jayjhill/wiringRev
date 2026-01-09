using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;
using WireModelingPlugin.Utils;

namespace WireModelingPlugin.Commands;

/// <summary>
/// Command to draw wire segments point-to-point in any view.
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class DrawWireCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        // Validate view type
        if (!IsValidViewForWirePlacement(view))
        {
            TaskDialog.Show("Wire Modeling",
                "Wire can only be placed in Floor Plan, Section, Elevation, or 3D views.");
            return Result.Cancelled;
        }

        try
        {
            var settings = PluginSettings.Instance;
            var wireConfig = settings.DefaultWireConfiguration.Clone();

            // Show wire properties dialog first
            var propertiesDialog = new UI.WirePropertiesDialog(wireConfig);
            if (propertiesDialog.ShowDialog() != true)
            {
                return Result.Cancelled;
            }

            wireConfig = propertiesDialog.Configuration;

            // Start drawing mode
            var pathPoints = new List<XYZ>();
            bool continueDrawing = true;

            TaskDialog.Show("Wire Modeling",
                "Click to place wire path points.\n" +
                "Press ESC or right-click to finish the current wire segment.");

            while (continueDrawing)
            {
                try
                {
                    // Pick point for wire path
                    XYZ pickedPoint = uiDoc.Selection.PickPoint(
                        ObjectSnapTypes.Endpoints | ObjectSnapTypes.Midpoints |
                        ObjectSnapTypes.Intersections | ObjectSnapTypes.Nearest,
                        $"Pick wire path point {pathPoints.Count + 1} (ESC to finish)");

                    // Check for connector snap if enabled
                    if (settings.AutoSnapToConnectors)
                    {
                        var snappedPoint = TrySnapToConnector(doc, pickedPoint, settings.SnapTolerance);
                        if (snappedPoint != null)
                        {
                            pickedPoint = snappedPoint.Value;
                        }
                    }

                    pathPoints.Add(pickedPoint);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    // User pressed ESC or right-clicked
                    continueDrawing = false;
                }
            }

            // Need at least 2 points to create a wire segment
            if (pathPoints.Count < 2)
            {
                TaskDialog.Show("Wire Modeling", "At least 2 points are required to create a wire segment.");
                return Result.Cancelled;
            }

            // Create the wire segment
            using var transaction = new Transaction(doc, "Draw Wire Segment");
            transaction.Start();

            var segment = new WireSegment
            {
                Configuration = wireConfig,
                PathPoints = pathPoints
            };

            // Create the geometry in Revit
            var wireElement = CreateWireGeometry(doc, segment, view);

            if (wireElement != null)
            {
                segment.RevitElementId = wireElement.Id;

                // Calculate length
                segment.CalculateTotalLength(settings.LengthCalculation);

                // Store segment data
                var dataService = GetOrCreateDataService(doc);
                dataService.AddSegment(segment);

                // Set Revit parameters
                SetWireParameters(wireElement, segment);

                // Add to recent configurations
                settings.AddRecentConfiguration(wireConfig);
            }

            transaction.Commit();

            // Show summary
            TaskDialog.Show("Wire Modeling",
                $"Wire segment created:\n" +
                $"Type: {wireConfig.GetSpecificationString()}\n" +
                $"Points: {pathPoints.Count}\n" +
                $"Length: {segment.TotalLength:F2} ft");

            return Result.Succeeded;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }

    private bool IsValidViewForWirePlacement(View view)
    {
        return view.ViewType switch
        {
            ViewType.FloorPlan => true,
            ViewType.CeilingPlan => true,
            ViewType.Section => true,
            ViewType.Elevation => true,
            ViewType.ThreeD => true,
            _ => false
        };
    }

    private XYZ? TrySnapToConnector(Document doc, XYZ point, double tolerance)
    {
        // Find nearby electrical connectors
        var collector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
            .WhereElementIsNotElementType();

        foreach (var element in collector)
        {
            var location = element.Location as LocationPoint;
            if (location != null)
            {
                var distance = point.DistanceTo(location.Point);
                if (distance <= tolerance)
                {
                    return location.Point;
                }
            }
        }

        // Also check electrical equipment (panels)
        var panelCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
            .WhereElementIsNotElementType();

        foreach (var element in panelCollector)
        {
            var location = element.Location as LocationPoint;
            if (location != null)
            {
                var distance = point.DistanceTo(location.Point);
                if (distance <= tolerance)
                {
                    return location.Point;
                }
            }
        }

        return null;
    }

    private Element? CreateWireGeometry(Document doc, WireSegment segment, View view)
    {
        // Create a DirectShape element to represent the wire
        var categoryId = new ElementId(BuiltInCategory.OST_GenericModel);

        // Get display color
        var (r, g, b) = WireColorService.GetRevitColor(segment.Configuration);

        // Create geometry builder
        var builder = new TessellatedShapeBuilder();
        builder.OpenConnectedFaceSet(false);

        // Create wire path as a series of connected line segments with thickness
        var wireRadius = GetWireRadius(segment.Configuration.Gauge);

        for (int i = 0; i < segment.PathPoints.Count - 1; i++)
        {
            var start = segment.PathPoints[i];
            var end = segment.PathPoints[i + 1];

            // Create a simple rectangular extrusion for visualization
            var direction = (end - start).Normalize();
            var perpendicular = GetPerpendicular(direction);

            // Create wire cross-section vertices
            var offset1 = perpendicular * wireRadius;
            var offset2 = XYZ.BasisZ.CrossProduct(direction).Normalize() * wireRadius;

            // Create faces for the wire segment (simplified box)
            var p1 = start + offset1 + offset2;
            var p2 = start + offset1 - offset2;
            var p3 = start - offset1 - offset2;
            var p4 = start - offset1 + offset2;
            var p5 = end + offset1 + offset2;
            var p6 = end + offset1 - offset2;
            var p7 = end - offset1 - offset2;
            var p8 = end - offset1 + offset2;

            // Add faces
            builder.AddFace(new TessellatedFace(new List<XYZ> { p1, p2, p3, p4 }, ElementId.InvalidElementId));
            builder.AddFace(new TessellatedFace(new List<XYZ> { p5, p8, p7, p6 }, ElementId.InvalidElementId));
            builder.AddFace(new TessellatedFace(new List<XYZ> { p1, p5, p6, p2 }, ElementId.InvalidElementId));
            builder.AddFace(new TessellatedFace(new List<XYZ> { p2, p6, p7, p3 }, ElementId.InvalidElementId));
            builder.AddFace(new TessellatedFace(new List<XYZ> { p3, p7, p8, p4 }, ElementId.InvalidElementId));
            builder.AddFace(new TessellatedFace(new List<XYZ> { p4, p8, p5, p1 }, ElementId.InvalidElementId));
        }

        builder.CloseConnectedFaceSet();
        builder.Build();

        var result = builder.GetBuildResult();
        if (result.Outcome == TessellatedShapeBuilderOutcome.Nothing)
        {
            // Fall back to model lines if solid creation fails
            return CreateWireAsModelLines(doc, segment, view);
        }

        var directShape = DirectShape.CreateElement(doc, categoryId);
        directShape.SetShape(result.GetGeometricalObjects().ToList());
        directShape.Name = $"Wire-{segment.Configuration.GetSpecificationString()}";

        // Apply color override
        var overrideSettings = new OverrideGraphicSettings();
        overrideSettings.SetProjectionLineColor(new Color(r, g, b));
        overrideSettings.SetSurfaceForegroundPatternColor(new Color(r, g, b));
        view.SetElementOverrides(directShape.Id, overrideSettings);

        return directShape;
    }

    private Element? CreateWireAsModelLines(Document doc, WireSegment segment, View view)
    {
        // Create model lines as fallback visualization
        var sketchPlane = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(
            view.ViewDirection, segment.PathPoints[0]));

        var curveArray = new List<Curve>();
        for (int i = 0; i < segment.PathPoints.Count - 1; i++)
        {
            var line = Line.CreateBound(segment.PathPoints[i], segment.PathPoints[i + 1]);
            curveArray.Add(line);
        }

        // Create model curves
        if (curveArray.Count > 0)
        {
            var modelCurve = doc.Create.NewModelCurve(curveArray[0], sketchPlane);

            // Set line style if available
            var (r, g, b) = WireColorService.GetRevitColor(segment.Configuration);
            var overrideSettings = new OverrideGraphicSettings();
            overrideSettings.SetProjectionLineColor(new Color(r, g, b));
            view.SetElementOverrides(modelCurve.Id, overrideSettings);

            return modelCurve;
        }

        return null;
    }

    private double GetWireRadius(WireGauge gauge)
    {
        // Return wire radius in feet based on gauge (approximate for visualization)
        return gauge switch
        {
            WireGauge.AWG_14 => 0.005,
            WireGauge.AWG_12 => 0.006,
            WireGauge.AWG_10 => 0.008,
            WireGauge.AWG_8 => 0.010,
            WireGauge.AWG_6 => 0.013,
            WireGauge.AWG_4 => 0.017,
            WireGauge.AWG_2 => 0.021,
            WireGauge.AWG_1 => 0.024,
            WireGauge.AWG_1_0 => 0.027,
            WireGauge.AWG_2_0 => 0.030,
            WireGauge.AWG_3_0 => 0.034,
            WireGauge.AWG_4_0 => 0.038,
            _ => 0.006
        };
    }

    private XYZ GetPerpendicular(XYZ direction)
    {
        // Get a perpendicular vector to the direction
        if (Math.Abs(direction.Z) < 0.9)
        {
            return XYZ.BasisZ.CrossProduct(direction).Normalize();
        }
        else
        {
            return XYZ.BasisX.CrossProduct(direction).Normalize();
        }
    }

    private void SetWireParameters(Element element, WireSegment segment)
    {
        // Set custom parameters on the wire element
        var config = segment.Configuration;

        // Try to set parameters if they exist
        SetParameter(element, "Wire Type", config.InsulationType.GetDisplayName());
        SetParameter(element, "Wire Gauge", config.Gauge.GetDisplayName());
        SetParameter(element, "Conductor Count", config.ConductorCount);
        SetParameter(element, "Ground Size", config.GroundSize.GetDisplayName());
        SetParameter(element, "Voltage Rating", config.VoltageRating.GetDisplayName());
        SetParameter(element, "Temperature Rating", config.TemperatureRating.GetDisplayName());
        SetParameter(element, "Wire Specification", config.GetSpecificationString());
        SetParameter(element, "Wire Length", segment.TotalLength);
    }

    private void SetParameter(Element element, string paramName, object value)
    {
        var param = element.LookupParameter(paramName);
        if (param != null && !param.IsReadOnly)
        {
            switch (value)
            {
                case string s:
                    param.Set(s);
                    break;
                case int i:
                    param.Set(i);
                    break;
                case double d:
                    param.Set(d);
                    break;
            }
        }
    }

    private WireDataService GetOrCreateDataService(Document doc)
    {
        // In a real implementation, this would use ExtensibleStorage or
        // another mechanism to persist data with the document
        return new WireDataService(doc);
    }
}
