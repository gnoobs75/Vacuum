using Godot;
using Vacuum.Core;
using Vacuum.Systems.Flight;

namespace Vacuum.Systems.Navigation;

/// <summary>
/// WO-33: Adapted from WebSocket real-time sync to local signal-based position broadcasting.
/// Attach as child of ShipPhysics. Periodically updates NavigationService with ship position.
/// </summary>
public partial class ShipPositionSync : Node
{
    [Export] public float SyncIntervalSeconds { get; set; } = 0.1f;
    [Export] public string ShipId { get; set; } = "player_ship";

    private ShipPhysics? _ship;
    private float _timer;

    public override void _Ready()
    {
        _ship = GetParentOrNull<ShipPhysics>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ship == null) return;

        _timer += (float)delta;
        if (_timer < SyncIntervalSeconds) return;
        _timer = 0f;

        var pos = _ship.GlobalPosition;
        var vel = _ship.LinearVelocity;
        var rot = _ship.Rotation;

        NavigationService.Instance?.UpdateShipPosition(ShipId, pos, vel, rot);
        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.ShipPositionUpdated,
            ShipId, pos, vel);
    }
}
