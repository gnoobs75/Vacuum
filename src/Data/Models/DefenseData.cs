using System;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-91: Shield layer state.
/// </summary>
public class ShieldStateData
{
    public string ShieldId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public float CurrentHp { get; set; } = 100f;
    public float MaxHp { get; set; } = 100f;
    public float RegenerationRate { get; set; } = 1f;
    public DateTime? LastDamaged { get; set; }
}

/// <summary>
/// WO-91: Armor layer state.
/// </summary>
public class ArmorStateData
{
    public string ArmorId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public float CurrentHp { get; set; } = 100f;
    public float MaxHp { get; set; } = 100f;
    public float RepairRate { get; set; } = 0f;
    public DateTime? LastRepaired { get; set; }
}

/// <summary>
/// WO-91: Hull integrity state.
/// </summary>
public class HullStateData
{
    public string HullId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public float CurrentHp { get; set; } = 100f;
    public float MaxHp { get; set; } = 100f;
    public bool Breached { get; set; } = false;
}

/// <summary>
/// WO-91: Damage resistance profile for a ship against a specific damage type.
/// </summary>
public class DamageResistanceData
{
    public string ResistanceId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public DamageType DamageType { get; set; }
    public float ResistancePercentage { get; set; }
}

/// <summary>
/// WO-91: Active repair module on a ship.
/// </summary>
public class RepairModuleData
{
    public string ModuleId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public float RepairAmount { get; set; } = 10f;
    public float CycleTime { get; set; } = 5f;
    public bool Active { get; set; } = false;
}
