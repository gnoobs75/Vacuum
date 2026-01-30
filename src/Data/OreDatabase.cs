using System.Collections.Generic;
using Vacuum.Data.Models;

namespace Vacuum.Data;

/// <summary>
/// Static database of all ore and mineral definitions.
/// </summary>
public static class OreDatabase
{
    public static readonly Dictionary<string, OreDefinition> Ores = new()
    {
        ["veldspar"] = new OreDefinition
        {
            OreId = "veldspar", Name = "Veldspar", Rarity = OreRarity.Common,
            BaseYield = 150f, Volume = 0.1f, BaseValue = 8f, ReprocessBatchSize = 100,
            MineralYield = new() { ["tritanium"] = 415f }
        },
        ["scordite"] = new OreDefinition
        {
            OreId = "scordite", Name = "Scordite", Rarity = OreRarity.Common,
            BaseYield = 130f, Volume = 0.15f, BaseValue = 12f, ReprocessBatchSize = 100,
            MineralYield = new() { ["tritanium"] = 346f, ["pyerite"] = 173f }
        },
        ["pyroxeres"] = new OreDefinition
        {
            OreId = "pyroxeres", Name = "Pyroxeres", Rarity = OreRarity.Uncommon,
            BaseYield = 100f, Volume = 0.3f, BaseValue = 20f, ReprocessBatchSize = 100,
            MineralYield = new() { ["pyerite"] = 120f, ["mexallon"] = 60f }
        },
        ["plagioclase"] = new OreDefinition
        {
            OreId = "plagioclase", Name = "Plagioclase", Rarity = OreRarity.Uncommon,
            BaseYield = 90f, Volume = 0.35f, BaseValue = 25f, ReprocessBatchSize = 100,
            MineralYield = new() { ["tritanium"] = 256f, ["mexallon"] = 128f }
        },
        ["kernite"] = new OreDefinition
        {
            OreId = "kernite", Name = "Kernite", Rarity = OreRarity.Moderate,
            BaseYield = 70f, Volume = 1.2f, BaseValue = 45f, ReprocessBatchSize = 100,
            MineralYield = new() { ["mexallon"] = 267f, ["isogen"] = 134f }
        },
        ["jaspet"] = new OreDefinition
        {
            OreId = "jaspet", Name = "Jaspet", Rarity = OreRarity.Rare,
            BaseYield = 50f, Volume = 2f, BaseValue = 80f, ReprocessBatchSize = 100,
            MineralYield = new() { ["mexallon"] = 350f, ["zydrine"] = 8f }
        },
        ["hemorphite"] = new OreDefinition
        {
            OreId = "hemorphite", Name = "Hemorphite", Rarity = OreRarity.Rare,
            BaseYield = 40f, Volume = 3f, BaseValue = 100f, ReprocessBatchSize = 100,
            MineralYield = new() { ["isogen"] = 212f, ["nocxium"] = 22f }
        },
        ["hedbergite"] = new OreDefinition
        {
            OreId = "hedbergite", Name = "Hedbergite", Rarity = OreRarity.VeryRare,
            BaseYield = 25f, Volume = 3f, BaseValue = 180f, ReprocessBatchSize = 100,
            MineralYield = new() { ["zydrine"] = 19f, ["megacyte"] = 3f }
        },
        ["mercoxit"] = new OreDefinition
        {
            OreId = "mercoxit", Name = "Mercoxit", Rarity = OreRarity.ExtremelyRare,
            BaseYield = 15f, Volume = 40f, BaseValue = 500f, ReprocessBatchSize = 100,
            MineralYield = new() { ["morphite"] = 14f }
        }
    };

    public static readonly Dictionary<string, MineralDefinition> Minerals = new()
    {
        ["tritanium"] = new() { MineralId = "tritanium", Name = "Tritanium", Volume = 0.01f, BaseValue = 5f },
        ["pyerite"] = new() { MineralId = "pyerite", Name = "Pyerite", Volume = 0.01f, BaseValue = 10f },
        ["mexallon"] = new() { MineralId = "mexallon", Name = "Mexallon", Volume = 0.01f, BaseValue = 30f },
        ["isogen"] = new() { MineralId = "isogen", Name = "Isogen", Volume = 0.01f, BaseValue = 60f },
        ["nocxium"] = new() { MineralId = "nocxium", Name = "Nocxium", Volume = 0.01f, BaseValue = 150f },
        ["zydrine"] = new() { MineralId = "zydrine", Name = "Zydrine", Volume = 0.01f, BaseValue = 500f },
        ["megacyte"] = new() { MineralId = "megacyte", Name = "Megacyte", Volume = 0.01f, BaseValue = 1200f },
        ["morphite"] = new() { MineralId = "morphite", Name = "Morphite", Volume = 0.01f, BaseValue = 5000f }
    };

    /// <summary>
    /// Returns ore types appropriate for a given distance from the star.
    /// Inner = common, outer = rare.
    /// </summary>
    public static string[] GetOresForDistance(float distance, float beltRadius)
    {
        float ratio = distance / beltRadius;
        if (ratio < 0.8f)
            return new[] { "veldspar", "scordite" };
        if (ratio < 1.0f)
            return new[] { "veldspar", "scordite", "pyroxeres", "plagioclase" };
        if (ratio < 1.1f)
            return new[] { "pyroxeres", "plagioclase", "kernite" };
        if (ratio < 1.3f)
            return new[] { "kernite", "jaspet", "hemorphite" };
        return new[] { "hemorphite", "hedbergite", "mercoxit" };
    }
}
