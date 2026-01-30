using Godot;
using System.Collections.Generic;
using Vacuum.Data;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Core mining laser mechanics. Activates with F1 key when a target asteroid is locked.
/// Runs mining cycles, extracts ore, deposits into cargo hold.
/// </summary>
public partial class MiningLaserSystem : Node
{
    [Export] public float CycleTime { get; set; } = 5f; // seconds per mining cycle
    [Export] public float BaseYieldPerCycle { get; set; } = 20f; // units per cycle
    [Export] public float LaserRange { get; set; } = 150f;
    [Export] public int MaxLasers { get; set; } = 2;
    [Export] public NodePath ShipPath { get; set; } = "";
    [Export] public NodePath TargetingPath { get; set; } = "";
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private Node3D? _ship;
    private TargetingSystem? _targeting;
    private CargoHold? _cargoHold;

    private bool _active;
    private float _cycleProgress; // 0..CycleTime
    private int _activeLaserCount = 1;
    private MiningLaserEffect? _laserEffect;

    // Stats
    private int _totalOreMinedSession;
    private int _totalCyclesCompleted;
    private float _totalTimeMinedSession;

    public bool IsActive => _active;
    public float CycleProgress => _cycleProgress;
    public float CyclePercent => CycleTime > 0 ? _cycleProgress / CycleTime * 100f : 0f;
    public int ActiveLaserCount => _activeLaserCount;
    public int TotalOreMined => _totalOreMinedSession;
    public int TotalCycles => _totalCyclesCompleted;
    public float TotalTimeMined => _totalTimeMinedSession;
    public float YieldPerMinute => _totalTimeMinedSession > 0 ? _totalOreMinedSession / (_totalTimeMinedSession / 60f) : 0f;
    public string? CurrentOreId { get; private set; }

    [Signal]
    public delegate void MiningStartedEventHandler();

    [Signal]
    public delegate void MiningStoppedEventHandler();

    [Signal]
    public delegate void MiningCycleCompleteEventHandler(string oreId, int yield);

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(ShipPath))
            _ship = GetNodeOrNull<Node3D>(ShipPath);
        if (!string.IsNullOrEmpty(TargetingPath))
            _targeting = GetNodeOrNull<TargetingSystem>(TargetingPath);
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.F1)
        {
            if (_active)
                StopMining();
            else
                StartMining();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!_active) return;

        float dt = (float)delta;

        // Check target still valid and in range
        if (_targeting == null || !_targeting.HasTarget || _ship == null)
        {
            StopMining();
            return;
        }

        float dist = _ship.GlobalPosition.DistanceTo(_targeting.LockedTarget!.GlobalPosition);
        if (dist > LaserRange)
        {
            StopMining();
            return;
        }

        // Check if asteroid is depleted
        var asteroidNode = _targeting.LockedTarget;
        var asteroidState = GetAsteroidState(asteroidNode);
        if (asteroidState == null || asteroidState.Depleted)
        {
            StopMining();
            return;
        }

        // Check cargo full
        if (_cargoHold != null && _cargoHold.IsFull)
        {
            StopMining();
            return;
        }

        _totalTimeMinedSession += dt;
        _cycleProgress += dt;

        if (_cycleProgress >= CycleTime)
        {
            CompleteCycle(asteroidState);
            _cycleProgress = 0f;
        }
    }

    private void StartMining()
    {
        if (_targeting == null || !_targeting.HasTarget || _ship == null) return;

        float dist = _ship.GlobalPosition.DistanceTo(_targeting.LockedTarget!.GlobalPosition);
        if (dist > LaserRange) return;

        var state = GetAsteroidState(_targeting.LockedTarget);
        if (state == null || state.Depleted) return;

        _active = true;
        _cycleProgress = 0f;
        CurrentOreId = state.OreId;

        // Create visual laser effect
        CreateLaserEffect();

        EmitSignal(SignalName.MiningStarted);
    }

    public void StopMining()
    {
        if (!_active) return;
        _active = false;
        _cycleProgress = 0f;
        CurrentOreId = null;

        DestroyLaserEffect();

        EmitSignal(SignalName.MiningStopped);
    }

    private void CompleteCycle(AsteroidState state)
    {
        if (_cargoHold == null) return;

        float yield = BaseYieldPerCycle * _activeLaserCount;
        int oreToExtract = (int)Mathf.Min(yield, state.RemainingOre);

        if (oreToExtract <= 0)
        {
            StopMining();
            return;
        }

        // Get ore definition for volume
        float volumePerUnit = 0.1f;
        if (OreDatabase.Ores.TryGetValue(state.OreId, out var oreDef))
            volumePerUnit = oreDef.Volume;

        string name = oreDef?.Name ?? state.OreId;
        int added = _cargoHold.AddItem(state.OreId, name, oreToExtract, volumePerUnit);

        state.RemainingOre -= added;
        _totalOreMinedSession += added;
        _totalCyclesCompleted++;

        EmitSignal(SignalName.MiningCycleComplete, state.OreId, added);

        if (state.Depleted)
            StopMining();
    }

    private AsteroidState? GetAsteroidState(Node3D? node)
    {
        if (node == null) return null;
        // Look for MineableAsteroid component
        var mineable = node.GetNodeOrNull<MineableAsteroid>("MineableAsteroid");
        return mineable?.State;
    }

    private void CreateLaserEffect()
    {
        DestroyLaserEffect();
        if (_ship == null || _targeting?.LockedTarget == null) return;

        _laserEffect = new MiningLaserEffect();
        _laserEffect.Initialize(_ship, _targeting.LockedTarget);
        GetTree().Root.FindChild("Main", true, false)?.AddChild(_laserEffect);
    }

    private void DestroyLaserEffect()
    {
        if (_laserEffect != null && IsInstanceValid(_laserEffect))
        {
            _laserEffect.QueueFree();
            _laserEffect = null;
        }
    }
}
