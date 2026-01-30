using Godot;
using Vacuum.Core;
using Vacuum.Data.Enums;

namespace Vacuum.Systems.Navigation;

/// <summary>
/// WO-40: Warp drive mechanics - alignment, acceleration, warp tunnel, deceleration.
/// Attach as a child of the ShipPhysics node.
/// </summary>
public partial class WarpDriveSystem : Node
{
    [Export] public float AlignTime { get; set; } = 3f;
    [Export] public float WarpSpeed { get; set; } = 3000f;
    [Export] public float WarpExitDistance { get; set; } = 50f;
    [Export] public float MinWarpDistance { get; set; } = 150f;

    public WarpState State { get; private set; } = WarpState.Idle;
    public Vector3 WarpDestination { get; private set; }
    public float WarpProgress { get; private set; }

    private RigidBody3D? _ship;
    private float _alignProgress;
    private float _warpDistanceTotal;
    private float _warpDistanceTravelled;

    [Signal] public delegate void WarpStateChangedEventHandler(int state);
    [Signal] public delegate void WarpProgressUpdatedEventHandler(float progress);

    public override void _Ready()
    {
        _ship = GetParentOrNull<RigidBody3D>();
    }

    public bool InitiateWarp(Vector3 destination)
    {
        if (_ship == null || State != WarpState.Idle) return false;

        float distance = _ship.GlobalPosition.DistanceTo(destination);
        if (distance < MinWarpDistance) return false;

        WarpDestination = destination;
        _warpDistanceTotal = distance;
        _warpDistanceTravelled = 0;
        _alignProgress = 0;
        State = WarpState.Aligning;
        EmitSignal(SignalName.WarpStateChanged, (int)State);
        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WarpInitiated,
            _ship.Name, destination);
        return true;
    }

    public void CancelWarp()
    {
        if (State == WarpState.Aligning)
        {
            State = WarpState.Idle;
            EmitSignal(SignalName.WarpStateChanged, (int)State);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ship == null) return;
        float dt = (float)delta;

        switch (State)
        {
            case WarpState.Aligning:
                ProcessAlignment(dt);
                break;
            case WarpState.Accelerating:
                ProcessAcceleration(dt);
                break;
            case WarpState.InWarp:
                ProcessWarp(dt);
                break;
            case WarpState.Decelerating:
                ProcessDeceleration(dt);
                break;
        }
    }

    private void ProcessAlignment(float dt)
    {
        // Rotate ship to face destination
        Vector3 direction = (WarpDestination - _ship!.GlobalPosition).Normalized();
        var targetBasis = Basis.LookingAt(direction, Vector3.Up);
        _ship.GlobalTransform = new Transform3D(
            _ship.GlobalTransform.Basis.Slerp(targetBasis, dt * 2f),
            _ship.GlobalTransform.Origin
        );

        _alignProgress += dt / AlignTime;
        if (_alignProgress >= 1f)
        {
            _ship.LinearVelocity = Vector3.Zero;
            State = WarpState.Accelerating;
            EmitSignal(SignalName.WarpStateChanged, (int)State);
        }
    }

    private void ProcessAcceleration(float dt)
    {
        // Quick ramp to warp speed
        Vector3 direction = (WarpDestination - _ship!.GlobalPosition).Normalized();
        _ship.LinearVelocity = direction * WarpSpeed * Mathf.Min(_alignProgress + dt * 3f, 1f);
        _alignProgress += dt * 3f;
        if (_alignProgress >= 2f)
        {
            State = WarpState.InWarp;
            EmitSignal(SignalName.WarpStateChanged, (int)State);
        }
    }

    private void ProcessWarp(float dt)
    {
        Vector3 direction = (WarpDestination - _ship!.GlobalPosition).Normalized();
        _ship.LinearVelocity = direction * WarpSpeed;
        _warpDistanceTravelled += WarpSpeed * dt;

        WarpProgress = _warpDistanceTravelled / _warpDistanceTotal;
        EmitSignal(SignalName.WarpProgressUpdated, WarpProgress);

        float remaining = _ship.GlobalPosition.DistanceTo(WarpDestination);
        if (remaining < WarpExitDistance * 3f)
        {
            State = WarpState.Decelerating;
            EmitSignal(SignalName.WarpStateChanged, (int)State);
        }
    }

    private void ProcessDeceleration(float dt)
    {
        float remaining = _ship!.GlobalPosition.DistanceTo(WarpDestination);
        float speedFactor = Mathf.Max(remaining / (WarpExitDistance * 3f), 0.01f);
        Vector3 direction = (WarpDestination - _ship.GlobalPosition).Normalized();
        _ship.LinearVelocity = direction * WarpSpeed * speedFactor;

        if (remaining < WarpExitDistance)
        {
            _ship.LinearVelocity = Vector3.Zero;
            State = WarpState.Idle;
            WarpProgress = 1f;
            EmitSignal(SignalName.WarpStateChanged, (int)State);
            EmitSignal(SignalName.WarpProgressUpdated, 1f);
            GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WarpCompleted,
                _ship.Name, _ship.GlobalPosition);
        }
    }
}
