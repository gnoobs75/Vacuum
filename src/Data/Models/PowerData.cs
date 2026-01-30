using System;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-69: Reactor providing power to ship systems.
/// </summary>
public class ReactorData
{
    public string ReactorId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public string ReactorType { get; set; } = "Standard";
    public float PowerOutput { get; set; } = 100f;
    public float OverloadBonus { get; set; } = 0.25f;
    public float OverloadHeat { get; set; } = 10f;
    public bool Active { get; set; } = true;
}

/// <summary>
/// WO-69: Capacitor storing energy for module activation.
/// </summary>
public class CapacitorData
{
    public string CapacitorId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public float Capacity { get; set; } = 500f;
    public float CurrentCharge { get; set; } = 500f;
    public float RechargeRate { get; set; } = 5f;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-69: Power consumption record for a module.
/// </summary>
public class ModulePowerConsumptionData
{
    public string ConsumptionId { get; set; } = Guid.NewGuid().ToString();
    public string ModuleId { get; set; } = string.Empty;
    public float PowerConsumption { get; set; }
    public float CpuConsumption { get; set; }
}

/// <summary>
/// WO-69: Active power overload state on a ship.
/// </summary>
public class PowerOverloadData
{
    public string OverloadId { get; set; } = Guid.NewGuid().ToString();
    public string ShipId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public int Duration { get; set; }
    public float DamageRisk { get; set; }
}
