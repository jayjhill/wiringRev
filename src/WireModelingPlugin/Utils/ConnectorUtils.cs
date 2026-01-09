using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

namespace WireModelingPlugin.Utils;

/// <summary>
/// Utility methods for working with electrical connectors.
/// </summary>
public static class ConnectorUtils
{
    /// <summary>
    /// Finds all electrical connectors near a given point.
    /// </summary>
    public static List<ConnectorInfo> FindNearbyConnectors(Document doc, XYZ point, double tolerance)
    {
        var connectors = new List<ConnectorInfo>();

        // Search electrical fixtures
        var fixtureCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
            .WhereElementIsNotElementType();

        foreach (var element in fixtureCollector)
        {
            var info = GetConnectorInfo(element, point, tolerance);
            if (info != null)
            {
                connectors.Add(info);
            }
        }

        // Search electrical equipment (panels)
        var equipmentCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
            .WhereElementIsNotElementType();

        foreach (var element in equipmentCollector)
        {
            var info = GetConnectorInfo(element, point, tolerance);
            if (info != null)
            {
                connectors.Add(info);
            }
        }

        // Search lighting fixtures
        var lightingCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_LightingFixtures)
            .WhereElementIsNotElementType();

        foreach (var element in lightingCollector)
        {
            var info = GetConnectorInfo(element, point, tolerance);
            if (info != null)
            {
                connectors.Add(info);
            }
        }

        return connectors.OrderBy(c => c.Distance).ToList();
    }

    /// <summary>
    /// Gets connector information for an element if it's within tolerance.
    /// </summary>
    private static ConnectorInfo? GetConnectorInfo(Element element, XYZ point, double tolerance)
    {
        var location = element.Location;

        if (location is LocationPoint locationPoint)
        {
            var distance = point.DistanceTo(locationPoint.Point);
            if (distance <= tolerance)
            {
                return new ConnectorInfo
                {
                    ElementId = element.Id,
                    ElementName = element.Name,
                    Location = locationPoint.Point,
                    Distance = distance,
                    ConnectorType = GetConnectorType(element)
                };
            }
        }

        // Also check MEP connectors
        if (element is FamilyInstance familyInstance)
        {
            var mepModel = familyInstance.MEPModel;
            if (mepModel?.ConnectorManager != null)
            {
                foreach (Connector connector in mepModel.ConnectorManager.Connectors)
                {
                    if (connector.Domain == Domain.DomainElectrical)
                    {
                        var connectorPoint = connector.Origin;
                        var distance = point.DistanceTo(connectorPoint);

                        if (distance <= tolerance)
                        {
                            return new ConnectorInfo
                            {
                                ElementId = element.Id,
                                ElementName = element.Name,
                                Location = connectorPoint,
                                Distance = distance,
                                ConnectorType = GetConnectorType(element),
                                Connector = connector
                            };
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines the connector type based on the element category.
    /// </summary>
    private static ConnectorType GetConnectorType(Element element)
    {
        var category = element.Category?.BuiltInCategory;

        return category switch
        {
            BuiltInCategory.OST_ElectricalEquipment => ConnectorType.Panel,
            BuiltInCategory.OST_ElectricalFixtures => ConnectorType.Device,
            BuiltInCategory.OST_LightingFixtures => ConnectorType.Fixture,
            _ => ConnectorType.Unknown
        };
    }

    /// <summary>
    /// Gets the closest connector to a point.
    /// </summary>
    public static ConnectorInfo? GetClosestConnector(Document doc, XYZ point, double tolerance)
    {
        var connectors = FindNearbyConnectors(doc, point, tolerance);
        return connectors.FirstOrDefault();
    }

    /// <summary>
    /// Checks if a point is connected to an electrical element.
    /// </summary>
    public static bool IsPointConnected(Document doc, XYZ point, double tolerance)
    {
        return GetClosestConnector(doc, point, tolerance) != null;
    }
}

/// <summary>
/// Information about a detected electrical connector.
/// </summary>
public class ConnectorInfo
{
    public ElementId ElementId { get; set; } = ElementId.InvalidElementId;
    public string ElementName { get; set; } = string.Empty;
    public XYZ Location { get; set; } = XYZ.Zero;
    public double Distance { get; set; }
    public ConnectorType ConnectorType { get; set; }
    public Connector? Connector { get; set; }
}

/// <summary>
/// Types of electrical connectors.
/// </summary>
public enum ConnectorType
{
    Unknown,
    Panel,
    Device,
    Fixture,
    JunctionBox
}
