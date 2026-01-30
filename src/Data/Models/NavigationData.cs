using System;
using System.Collections.Generic;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-5: Ship position and movement state in space.
/// </summary>
public class ShipPositionData
{
    public string ShipId { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float VelocityZ { get; set; }
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float RotationZ { get; set; }
}

/// <summary>
/// WO-5: Saved location bookmark.
/// </summary>
public class BookmarkData
{
    public string BookmarkId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public string Folder { get; set; } = "Default";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-5: Wormhole connection between two systems.
/// </summary>
public class WormholeData
{
    public string WormholeId { get; set; } = Guid.NewGuid().ToString();
    public string SourceSystem { get; set; } = string.Empty;
    public string DestinationSystem { get; set; } = string.Empty;
    public float Stability { get; set; } = 1.0f;
    public float MassLimit { get; set; } = 1000f;
    public float MassUsed { get; set; } = 0f;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CollapseTime { get; set; }
    public WormholeClassification Classification { get; set; } = WormholeClassification.Stable;
}

/// <summary>
/// WO-5: Calculated route with waypoints.
/// </summary>
public class RouteData
{
    public string RouteId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public List<WaypointData> Waypoints { get; set; } = new();
    public float Distance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-5: Single waypoint in a route.
/// </summary>
public class WaypointData
{
    public string Name { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
