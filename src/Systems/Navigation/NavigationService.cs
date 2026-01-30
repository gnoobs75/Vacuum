using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Core;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Navigation;

/// <summary>
/// WO-18: Navigation system service (adapted from FastAPI endpoints to local service).
/// Manages ship positions, bookmarks, warp operations, and route calculation.
/// </summary>
public partial class NavigationService : Node
{
    public static NavigationService? Instance { get; private set; }

    private readonly Dictionary<string, ShipPositionData> _shipPositions = new();
    private readonly Dictionary<string, List<BookmarkData>> _bookmarks = new();
    private readonly Dictionary<string, RouteData> _activeRoutes = new();

    public override void _Ready()
    {
        Instance = this;
    }

    // -- Ship Position --

    public ShipPositionData GetShipPosition(string shipId)
    {
        return _shipPositions.GetValueOrDefault(shipId) ?? new ShipPositionData { ShipId = shipId };
    }

    public void UpdateShipPosition(string shipId, Vector3 position, Vector3 velocity, Vector3 rotation)
    {
        _shipPositions[shipId] = new ShipPositionData
        {
            ShipId = shipId,
            PositionX = position.X, PositionY = position.Y, PositionZ = position.Z,
            VelocityX = velocity.X, VelocityY = velocity.Y, VelocityZ = velocity.Z,
            RotationX = rotation.X, RotationY = rotation.Y, RotationZ = rotation.Z,
        };
    }

    // -- Bookmarks --

    public List<BookmarkData> GetBookmarks(string characterId)
    {
        return _bookmarks.GetValueOrDefault(characterId) ?? new List<BookmarkData>();
    }

    public BookmarkData CreateBookmark(string characterId, string name, Vector3 position, string folder = "Default")
    {
        var bookmark = new BookmarkData
        {
            CharacterId = characterId,
            Name = name,
            PositionX = position.X,
            PositionY = position.Y,
            PositionZ = position.Z,
            Folder = folder
        };

        if (!_bookmarks.ContainsKey(characterId))
            _bookmarks[characterId] = new();
        _bookmarks[characterId].Add(bookmark);

        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.BookmarkCreated,
            bookmark.BookmarkId, name, position);
        return bookmark;
    }

    public bool DeleteBookmark(string characterId, string bookmarkId)
    {
        if (!_bookmarks.TryGetValue(characterId, out var list)) return false;
        var removed = list.RemoveAll(b => b.BookmarkId == bookmarkId) > 0;
        if (removed)
            GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.BookmarkDeleted, bookmarkId);
        return removed;
    }

    // -- Route Calculation --

    public RouteData CalculateRoute(string characterId, Vector3 start, List<Vector3> waypoints)
    {
        var route = new RouteData
        {
            CharacterId = characterId,
            Distance = 0f
        };

        Vector3 prev = start;
        foreach (var wp in waypoints)
        {
            route.Waypoints.Add(new WaypointData { X = wp.X, Y = wp.Y, Z = wp.Z });
            route.Distance += prev.DistanceTo(wp);
            prev = wp;
        }

        _activeRoutes[characterId] = route;
        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.RouteCalculated, route.RouteId);
        return route;
    }

    public RouteData? GetActiveRoute(string characterId)
    {
        return _activeRoutes.GetValueOrDefault(characterId);
    }

    public void ClearRoute(string characterId)
    {
        _activeRoutes.Remove(characterId);
    }
}
