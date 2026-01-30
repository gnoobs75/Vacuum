using System;
using System.Collections.Generic;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-120: Ship instance owned by a character.
/// </summary>
public class ShipData
{
    public string ShipId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string ShipTypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Health { get; set; } = 100f;
    public float Shield { get; set; } = 100f;
    public float Armor { get; set; } = 100f;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ModuleSlotData> Slots { get; set; } = new();
}

/// <summary>
/// WO-120: Module definition.
/// </summary>
public class ModuleData
{
    public string ModuleId { get; set; } = Guid.NewGuid().ToString();
    public string ModuleTypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float PowerUsage { get; set; }
    public float CpuUsage { get; set; }
    public float HeatGeneration { get; set; }
}

/// <summary>
/// WO-120: Slot on a ship that can hold a module.
/// </summary>
public class ModuleSlotData
{
    public string SlotId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public SlotType SlotType { get; set; }
    public string? ModuleId { get; set; }
}
