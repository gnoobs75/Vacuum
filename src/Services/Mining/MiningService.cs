using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data;
using Vacuum.Data.Collections;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;
using Vacuum.Services.Interfaces;

namespace Vacuum.Services.Mining;

/// <summary>
/// Singleton service managing all mining-related operations and state.
/// </summary>
public partial class MiningService : Node, IMiningService
{
    public static MiningService? Instance { get; private set; }

    private readonly MiningCollections _data = new();

    [Signal] public delegate void MiningOperationStartedEventHandler(string operationId, string oreType);
    [Signal] public delegate void MiningOperationEndedEventHandler(string operationId, int totalYield);
    [Signal] public delegate void ReprocessingStartedEventHandler(string jobId, string oreType);
    [Signal] public delegate void ReprocessingCompletedEventHandler(string jobId);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[MiningService] Ready.");
    }

    public List<AsteroidData> GetNearbyAsteroids(float x, float y, float z, float radius, string? oreTypeFilter = null)
    {
        return _data.Asteroids.Where(a =>
        {
            float dx = a.PositionX - x;
            float dy = a.PositionY - y;
            float dz = a.PositionZ - z;
            float distSq = dx * dx + dy * dy + dz * dz;
            bool inRange = distSq <= radius * radius;
            bool matchesType = oreTypeFilter == null || a.OreType == oreTypeFilter;
            return inRange && matchesType;
        });
    }

    public MiningOperationData StartMiningOperation(string characterId, string asteroidId, string laserId)
    {
        var op = new MiningOperationData
        {
            CharacterId = characterId,
            AsteroidId = asteroidId,
            LaserId = laserId,
            Status = MiningStatus.Active
        };
        _data.Operations.Add(op);
        EmitSignal(SignalName.MiningOperationStarted, op.OperationId, "");
        return op;
    }

    public MiningOperationData? GetActiveOperation(string characterId)
    {
        return _data.Operations.FirstOrDefault(o =>
            o.CharacterId == characterId && o.Status == MiningStatus.Active);
    }

    public List<MiningOperationData> GetOperationHistory(string characterId, int limit = 50)
    {
        return _data.Operations.Where(o => o.CharacterId == characterId)
            .OrderByDescending(o => o.StartedAt)
            .Take(limit)
            .ToList();
    }

    public void StopMiningOperation(string operationId)
    {
        var op = _data.Operations.GetById(operationId);
        if (op == null || op.Status != MiningStatus.Active) return;

        op.Status = MiningStatus.Completed;
        op.EndedAt = DateTime.UtcNow;
        EmitSignal(SignalName.MiningOperationEnded, operationId, op.Yield);
    }

    public float CalculateYield(string oreType, float laserYield, float efficiency)
    {
        return laserYield * efficiency;
    }

    public ReprocessingJobData StartReprocessingJob(string characterId, string oreType, int quantity, float efficiency)
    {
        var job = new ReprocessingJobData
        {
            CharacterId = characterId,
            OreType = oreType,
            Quantity = quantity,
            Efficiency = efficiency,
            Status = ReprocessingJobStatus.Processing,
            Output = YieldCalculator.EstimateReprocessingOutput(oreType, quantity, efficiency)
        };
        _data.ReprocessingJobs.Add(job);
        EmitSignal(SignalName.ReprocessingStarted, job.JobId, oreType);
        return job;
    }

    public List<ReprocessingJobData> GetActiveReprocessingJobs(string characterId)
    {
        return _data.ReprocessingJobs.Where(j =>
            j.CharacterId == characterId && j.Status == ReprocessingJobStatus.Processing);
    }

    /// <summary>Register an asteroid in the service data layer.</summary>
    public void RegisterAsteroid(AsteroidData asteroid) => _data.Asteroids.Add(asteroid);

    /// <summary>Record yield from a mining cycle.</summary>
    public void RecordYield(string operationId, int amount)
    {
        var op = _data.Operations.GetById(operationId);
        if (op != null) op.Yield += amount;
    }
}
