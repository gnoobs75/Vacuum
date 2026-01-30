using System;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-101: Clone instance for a character.
/// </summary>
public class CloneData
{
    public string CloneId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsed { get; set; }
}

/// <summary>
/// WO-101: Pod (capsule) containing the character in space.
/// </summary>
public class PodData
{
    public string PodId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string ShipId { get; set; } = string.Empty;
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public DateTime? EjectedAt { get; set; }
    public PodStatus Status { get; set; } = PodStatus.Active;
}

/// <summary>
/// WO-101: Record of a character death event.
/// </summary>
public class DeathRecordData
{
    public string DeathId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string? ShipId { get; set; }
    public string? PodId { get; set; }
    public string? KillerId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double ShipLossValue { get; set; }
    public double CargoLossValue { get; set; }
}

/// <summary>
/// WO-101: Upgrade applied to a clone.
/// </summary>
public class CloneUpgradeData
{
    public string UpgradeId { get; set; } = Guid.NewGuid().ToString();
    public string CloneId { get; set; } = string.Empty;
    public string UpgradeType { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public float BonusValue { get; set; }
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}
