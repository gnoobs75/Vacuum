using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Allows the player to lock targets (asteroids, ships) with the T key.
/// Cycles through nearby targetable objects. Locked target is used by mining lasers.
/// </summary>
public partial class TargetingSystem : Node
{
    [Export] public NodePath ShipPath { get; set; } = "";
    [Export] public float MaxTargetRange { get; set; } = 200f;

    private Node3D? _ship;
    private Node3D? _lockedTarget;

    public Node3D? LockedTarget => _lockedTarget;
    public bool HasTarget => _lockedTarget != null && IsInstanceValid(_lockedTarget);

    [Signal]
    public delegate void TargetChangedEventHandler(Node3D? target);

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(ShipPath))
            _ship = GetNodeOrNull<Node3D>(ShipPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("target_lock"))
        {
            CycleTarget();
            GetViewport().SetInputAsHandled();
        }
    }

    private void CycleTarget()
    {
        if (_ship == null) return;

        // Find all asteroids with MineableAsteroid component in range
        var candidates = new List<Node3D>();
        var asteroidBelt = GetTree().Root.FindChild("AsteroidBelt", true, false);
        if (asteroidBelt != null)
        {
            foreach (var child in asteroidBelt.GetChildren())
            {
                if (child is Node3D node3d)
                {
                    float dist = _ship.GlobalPosition.DistanceTo(node3d.GlobalPosition);
                    if (dist <= MaxTargetRange)
                        candidates.Add(node3d);
                }
            }
        }

        if (candidates.Count == 0)
        {
            _lockedTarget = null;
            EmitSignal(SignalName.TargetChanged, default(Node3D)!);
            return;
        }

        // Sort by distance
        candidates.Sort((a, b) =>
            _ship.GlobalPosition.DistanceSquaredTo(a.GlobalPosition)
                .CompareTo(_ship.GlobalPosition.DistanceSquaredTo(b.GlobalPosition)));

        // Cycle to next target after current
        if (_lockedTarget != null && IsInstanceValid(_lockedTarget))
        {
            int idx = candidates.IndexOf(_lockedTarget);
            if (idx >= 0)
            {
                _lockedTarget = candidates[(idx + 1) % candidates.Count];
                EmitSignal(SignalName.TargetChanged, _lockedTarget);
                return;
            }
        }

        _lockedTarget = candidates[0];
        EmitSignal(SignalName.TargetChanged, _lockedTarget);
    }

    public float GetTargetDistance()
    {
        if (_ship == null || !HasTarget) return float.MaxValue;
        return _ship.GlobalPosition.DistanceTo(_lockedTarget!.GlobalPosition);
    }
}
