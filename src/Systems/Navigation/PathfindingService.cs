using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Vacuum.Core;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Navigation;

/// <summary>
/// WO-68: Background pathfinding service. Calculates safe routes,
/// avoids hazards, and caches frequently requested routes.
/// Runs heavy computation on background threads via BackgroundTaskProcessor.
/// </summary>
public partial class PathfindingService : Node
{
    public static PathfindingService? Instance { get; private set; }

    private readonly Dictionary<string, RouteData> _routeCache = new();

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Calculate route asynchronously via the background task processor.
    /// Returns a task ID to poll for completion.
    /// </summary>
    public string CalculateRouteAsync(string characterId, Vector3 start, Vector3 end,
        List<Vector3>? hazards = null)
    {
        string cacheKey = $"{start}_{end}";
        if (_routeCache.TryGetValue(cacheKey, out var cached))
        {
            NavigationService.Instance?.CalculateRoute(characterId, start, cached.Waypoints
                .Select(w => new Vector3(w.X, w.Y, w.Z)).ToList());
            return "cached";
        }

        var taskId = BackgroundTaskProcessor.Instance?.SubmitTask(
            $"Pathfinding {characterId}",
            async (ct) =>
            {
                var waypoints = ComputePath(start, end, hazards, ct);
                return waypoints;
            });

        return taskId ?? "error";
    }

    private List<Vector3> ComputePath(Vector3 start, Vector3 end,
        List<Vector3>? hazards, CancellationToken ct)
    {
        // Simple direct path with hazard avoidance
        var path = new List<Vector3>();
        Vector3 direction = (end - start).Normalized();
        float totalDistance = start.DistanceTo(end);

        if (hazards == null || hazards.Count == 0)
        {
            path.Add(end);
            return path;
        }

        // Basic hazard avoidance: add intermediate waypoints around hazards
        float step = 500f;
        Vector3 current = start;
        float hazardRadius = 200f;

        while (current.DistanceTo(end) > step)
        {
            ct.ThrowIfCancellationRequested();
            Vector3 next = current + direction * step;

            foreach (var hazard in hazards)
            {
                if (next.DistanceTo(hazard) < hazardRadius)
                {
                    // Offset perpendicular to avoid
                    Vector3 toHazard = (hazard - current).Normalized();
                    Vector3 perp = toHazard.Cross(Vector3.Up).Normalized();
                    next = hazard + perp * (hazardRadius + 50f);
                    path.Add(next);
                    break;
                }
            }

            current = next;
        }

        path.Add(end);
        return path;
    }
}
