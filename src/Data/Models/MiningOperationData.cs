using System;
using System.Collections.Generic;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// Persistent record of an asteroid in the game world.
/// </summary>
public class AsteroidData
{
    public string AsteroidId { get; set; } = Guid.NewGuid().ToString();
    public string SystemId { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public string OreType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime RespawnTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tracks an active or completed mining operation.
/// </summary>
public class MiningOperationData
{
    public string OperationId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string AsteroidId { get; set; } = string.Empty;
    public string LaserId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int Yield { get; set; }
    public MiningStatus Status { get; set; } = MiningStatus.Active;
}

/// <summary>
/// Mining laser module data for tracking equipped lasers.
/// </summary>
public class MiningLaserData
{
    public string LaserId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public float YieldPerCycle { get; set; } = 20f;
    public float CycleTime { get; set; } = 5f;
    public float HeatGeneration { get; set; } = 5f;
    public bool Active { get; set; }
}

/// <summary>
/// Tracks a reprocessing job converting ore to minerals.
/// </summary>
public class ReprocessingJobData
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string OreType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public float Efficiency { get; set; } = 0.7f;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public ReprocessingJobStatus Status { get; set; } = ReprocessingJobStatus.Queued;
    public Dictionary<string, int> Output { get; set; } = new();
}
