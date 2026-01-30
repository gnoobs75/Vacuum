using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Core;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Navigation;

/// <summary>
/// WO-150: Wormhole generation, stability mechanics, mass limits, transit, and collapse.
/// Manages all active wormholes in the game world.
/// </summary>
public partial class WormholeSystem : Node
{
    public static WormholeSystem? Instance { get; private set; }

    [Export] public float StabilityDecayRate { get; set; } = 0.01f;
    [Export] public float CollapseWarningThreshold { get; set; } = 0.3f;
    [Export] public float CriticalThreshold { get; set; } = 0.1f;

    private readonly Dictionary<string, WormholeData> _wormholes = new();
    private readonly Dictionary<string, Node3D?> _wormholeNodes = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        var toRemove = new List<string>();

        foreach (var (id, wh) in _wormholes)
        {
            // Decay stability over time
            wh.Stability -= StabilityDecayRate * dt;
            UpdateClassification(wh);

            if (wh.Stability <= CriticalThreshold && wh.Classification != WormholeClassification.Collapsing)
            {
                wh.Classification = WormholeClassification.Collapsing;
                GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WormholeCollapsing,
                    id, wh.Stability / StabilityDecayRate);
            }

            if (wh.Stability <= 0 || DateTime.UtcNow >= wh.CollapseTime)
            {
                toRemove.Add(id);
            }
        }

        foreach (var id in toRemove)
        {
            CollapseWormhole(id);
        }
    }

    public WormholeData SpawnWormhole(string sourceSystem, string destSystem,
        Vector3 position, float stability = 1f, float massLimit = 1000f, float lifetimeHours = 24f)
    {
        var wh = new WormholeData
        {
            SourceSystem = sourceSystem,
            DestinationSystem = destSystem,
            Stability = stability,
            MassLimit = massLimit,
            CollapseTime = DateTime.UtcNow.AddHours(lifetimeHours)
        };

        _wormholes[wh.WormholeId] = wh;
        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WormholeDiscovered, wh.WormholeId);
        return wh;
    }

    public bool TransitWormhole(string wormholeId, string shipId, float shipMass)
    {
        if (!_wormholes.TryGetValue(wormholeId, out var wh)) return false;
        if (wh.MassUsed + shipMass > wh.MassLimit) return false;
        if (wh.Classification == WormholeClassification.Collapsing) return false;

        wh.MassUsed += shipMass;
        // Mass transit reduces stability
        wh.Stability -= shipMass / wh.MassLimit * 0.2f;
        UpdateClassification(wh);

        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WormholeTransit, shipId, wormholeId);
        return true;
    }

    public WormholeData? GetWormhole(string wormholeId)
    {
        return _wormholes.GetValueOrDefault(wormholeId);
    }

    public List<WormholeData> GetWormholesInSystem(string systemId)
    {
        return _wormholes.Values
            .Where(w => w.SourceSystem == systemId || w.DestinationSystem == systemId)
            .ToList();
    }

    private void UpdateClassification(WormholeData wh)
    {
        wh.Classification = wh.Stability switch
        {
            > 0.7f => WormholeClassification.Stable,
            > 0.3f => WormholeClassification.Unstable,
            > 0.1f => WormholeClassification.Critical,
            _ => WormholeClassification.Collapsing
        };
    }

    private void CollapseWormhole(string wormholeId)
    {
        _wormholes.Remove(wormholeId);
        if (_wormholeNodes.TryGetValue(wormholeId, out var node))
        {
            node?.QueueFree();
            _wormholeNodes.Remove(wormholeId);
        }
        GameEventBus.Instance?.EmitSignal(GameEventBus.SignalName.WormholeCollapsed, wormholeId);
    }
}
