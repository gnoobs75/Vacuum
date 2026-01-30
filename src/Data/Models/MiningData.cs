using System.Collections.Generic;

namespace Vacuum.Data.Models;

/// <summary>
/// Defines an ore type that can be mined from asteroids.
/// </summary>
public class OreDefinition
{
    public string OreId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public OreRarity Rarity { get; set; } = OreRarity.Common;
    public float BaseYield { get; set; } = 100f;
    public float Volume { get; set; } = 0.1f; // m³ per unit
    public float BaseValue { get; set; } = 10f; // credits per unit
    public Dictionary<string, float> MineralYield { get; set; } = new(); // mineralId -> units per batch
    public int ReprocessBatchSize { get; set; } = 100;
}

/// <summary>
/// Defines a refined mineral produced from ore reprocessing.
/// </summary>
public class MineralDefinition
{
    public string MineralId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Volume { get; set; } = 0.01f;
    public float BaseValue { get; set; } = 50f;
}

/// <summary>
/// Runtime state of an asteroid that can be mined.
/// </summary>
public class AsteroidState
{
    public string OreId { get; set; } = string.Empty;
    public float TotalOre { get; set; } = 1000f;
    public float RemainingOre { get; set; } = 1000f;
    public float RespawnTimer { get; set; }
    public bool Depleted => RemainingOre <= 0f;
}

/// <summary>
/// An item stack in a cargo hold.
/// </summary>
public class CargoItem
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public float VolumePerUnit { get; set; }
    public float TotalVolume => Quantity * VolumePerUnit;
}

public enum OreRarity
{
    Common,
    Uncommon,
    Moderate,
    Rare,
    VeryRare,
    ExtremelyRare
}
