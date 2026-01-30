using System.Collections.Generic;
using Godot;
using Vacuum.Core;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;
using Vacuum.Systems.Flight;

namespace Vacuum.Systems.Navigation;

/// <summary>
/// WO-55: Autopilot system that follows a route of waypoints.
/// Attach as child of ShipPhysics.
/// </summary>
public partial class AutopilotSystem : Node
{
    [Export] public float WaypointArrivalDistance { get; set; } = 100f;
    [Export] public float AutopilotTurnRate { get; set; } = 1.5f;

    public bool Enabled { get; private set; }
    public int CurrentWaypointIndex { get; private set; }
    public RouteData? CurrentRoute { get; private set; }

    private ShipPhysics? _ship;
    private WarpDriveSystem? _warpDrive;

    [Signal] public delegate void WaypointReachedEventHandler(int index);
    [Signal] public delegate void RouteCompletedEventHandler();

    public override void _Ready()
    {
        _ship = GetParentOrNull<ShipPhysics>();
        _warpDrive = _ship?.GetNodeOrNull<WarpDriveSystem>("WarpDriveSystem");
    }

    public void SetRoute(RouteData route)
    {
        CurrentRoute = route;
        CurrentWaypointIndex = 0;
    }

    public void Toggle()
    {
        Enabled = !Enabled;
        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.AutopilotToggled,
            _ship?.Name ?? "", Enabled);
    }

    public void Enable() { Enabled = true; }
    public void Disable() { Enabled = false; }

    public override void _PhysicsProcess(double delta)
    {
        if (!Enabled || _ship == null || CurrentRoute == null) return;
        if (CurrentWaypointIndex >= CurrentRoute.Waypoints.Count)
        {
            Enabled = false;
            EmitSignal(SignalName.RouteCompleted);
            return;
        }

        var wp = CurrentRoute.Waypoints[CurrentWaypointIndex];
        var target = new Vector3(wp.X, wp.Y, wp.Z);
        float distance = _ship.GlobalPosition.DistanceTo(target);

        if (distance < WaypointArrivalDistance)
        {
            EmitSignal(SignalName.WaypointReached, CurrentWaypointIndex);
            CurrentWaypointIndex++;
            return;
        }

        // If far enough, use warp
        if (distance > (_warpDrive?.MinWarpDistance ?? 150f) && _warpDrive?.State == WarpState.Idle)
        {
            _warpDrive.InitiateWarp(target);
            return;
        }

        // Sublight autopilot: steer toward waypoint
        if (_warpDrive?.State != WarpState.Idle) return;

        Vector3 direction = (target - _ship.GlobalPosition).Normalized();
        _ship.SetThrustInput(new Vector3(0, 0, -1));
        _ship.SetRotationInput(Vector2.Zero);

        // Gradually rotate toward target
        var targetBasis = Basis.LookingAt(direction, Vector3.Up);
        float dt = (float)delta;
        _ship.GlobalTransform = new Transform3D(
            _ship.GlobalTransform.Basis.Slerp(targetBasis, dt * AutopilotTurnRate),
            _ship.GlobalTransform.Origin
        );
    }
}
