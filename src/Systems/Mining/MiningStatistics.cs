using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Tracks mining performance metrics: efficiency, yield rates, time spent, and per-ore breakdowns.
/// </summary>
public partial class MiningStatistics : Node
{
    public static MiningStatistics? Instance { get; private set; }

    // Session stats
    public float SessionStartTime { get; private set; }
    public float TotalMiningTime { get; private set; }
    public int TotalOreExtracted { get; private set; }
    public int TotalCyclesCompleted { get; private set; }
    public int TotalAsteroidsDepleted { get; private set; }
    public int ClaimJumpersEncountered { get; private set; }
    public int ClaimJumpersDefeated { get; private set; }

    // Per-ore tracking
    private readonly Dictionary<string, OreStats> _oreStats = new();

    // Lifetime stats
    public int LifetimeOreExtracted { get; private set; }
    public int LifetimeCycles { get; private set; }
    public float LifetimeMiningTime { get; private set; }

    // Efficiency
    public float YieldPerMinute => TotalMiningTime > 0 ? TotalOreExtracted / (TotalMiningTime / 60f) : 0f;
    public float CyclesPerMinute => TotalMiningTime > 0 ? TotalCyclesCompleted / (TotalMiningTime / 60f) : 0f;
    public float AverageYieldPerCycle => TotalCyclesCompleted > 0 ? (float)TotalOreExtracted / TotalCyclesCompleted : 0f;

    public float EstimatedIskPerHour
    {
        get
        {
            if (TotalMiningTime <= 0) return 0;
            float totalValue = _oreStats.Sum(kvp =>
            {
                if (OreDatabase.Ores.TryGetValue(kvp.Key, out var def))
                    return kvp.Value.Quantity * def.BaseValue;
                return 0f;
            });
            return totalValue / (TotalMiningTime / 3600f);
        }
    }

    [Signal] public delegate void StatisticsUpdatedEventHandler();

    public override void _Ready()
    {
        Instance = this;
        SessionStartTime = (float)Time.GetUnixTimeFromSystem();
        GD.Print("[MiningStatistics] Ready.");
    }

    public void RecordMiningCycle(string oreId, int yield, float cycleTime)
    {
        TotalOreExtracted += yield;
        TotalCyclesCompleted++;
        TotalMiningTime += cycleTime;
        LifetimeOreExtracted += yield;
        LifetimeCycles++;
        LifetimeMiningTime += cycleTime;

        if (!_oreStats.TryGetValue(oreId, out var stats))
        {
            stats = new OreStats { OreId = oreId };
            _oreStats[oreId] = stats;
        }
        stats.Quantity += yield;
        stats.Cycles++;
        stats.TotalTime += cycleTime;

        EmitSignal(SignalName.StatisticsUpdated);
    }

    public void RecordAsteroidDepleted() => TotalAsteroidsDepleted++;
    public void RecordClaimJumperEncounter() => ClaimJumpersEncountered++;
    public void RecordClaimJumperDefeated() => ClaimJumpersDefeated++;

    public void RecordMiningTime(float deltaTime)
    {
        TotalMiningTime += deltaTime;
        LifetimeMiningTime += deltaTime;
    }

    public IReadOnlyDictionary<string, OreStats> GetOreBreakdown() => _oreStats;

    public OreStats? GetOreStats(string oreId) =>
        _oreStats.TryGetValue(oreId, out var s) ? s : null;

    /// <summary>Get the most profitable ore mined this session.</summary>
    public string? GetMostProfitableOre()
    {
        string? best = null;
        float bestValue = 0;
        foreach (var (oreId, stats) in _oreStats)
        {
            if (OreDatabase.Ores.TryGetValue(oreId, out var def))
            {
                float value = stats.Quantity * def.BaseValue;
                if (value > bestValue) { bestValue = value; best = oreId; }
            }
        }
        return best;
    }

    public class OreStats
    {
        public string OreId { get; set; } = "";
        public int Quantity { get; set; }
        public int Cycles { get; set; }
        public float TotalTime { get; set; }
        public float AverageYield => Cycles > 0 ? (float)Quantity / Cycles : 0;
        public float YieldPerMinute => TotalTime > 0 ? Quantity / (TotalTime / 60f) : 0;
    }
}
