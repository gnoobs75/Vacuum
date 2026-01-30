using System;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

public class SovereigntyStructureData
{
    public string StructureId { get; set; } = Guid.NewGuid().ToString();
    public string StructureType { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public string OwnerId { get; set; } = string.Empty;
    public string FactionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TerritoryData
{
    public string TerritoryId { get; set; } = Guid.NewGuid().ToString();
    public string StructureId { get; set; } = string.Empty;
    public string ControllingFactionId { get; set; } = string.Empty;
    public string ControllingCharacterId { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastDefended { get; set; } = DateTime.UtcNow;
    public TerritoryStatus Status { get; set; } = TerritoryStatus.Controlled;
}

public class VulnerabilityTimerData
{
    public string TimerId { get; set; } = Guid.NewGuid().ToString();
    public string TerritoryId { get; set; } = string.Empty;
    public DateTime VulnerabilityStart { get; set; }
    public DateTime VulnerabilityEnd { get; set; }
    public int ReinforcementLevel { get; set; }
}

public class DefenseEventData
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string TerritoryId { get; set; } = string.Empty;
    public string DefendingFactionId { get; set; } = string.Empty;
    public string AttackingFactionId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public DefenseOutcome Outcome { get; set; } = DefenseOutcome.Contested;
}

public class StructureUpgradeData
{
    public string UpgradeId { get; set; } = Guid.NewGuid().ToString();
    public string StructureId { get; set; } = string.Empty;
    public string UpgradeType { get; set; } = string.Empty;
    public int Level { get; set; }
    public float BonusValue { get; set; }
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}
