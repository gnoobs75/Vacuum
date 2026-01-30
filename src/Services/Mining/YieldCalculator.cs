using Vacuum.Data;
using Vacuum.Data.Models;

namespace Vacuum.Services.Mining;

/// <summary>
/// Calculates mining yields based on ship fittings, skills, and asteroid properties.
/// </summary>
public static class YieldCalculator
{
    /// <summary>
    /// Calculate ore yield per cycle for a given laser and ore type.
    /// </summary>
    public static float CalculateCycleYield(float baseYieldPerCycle, int laserCount, float skillMultiplier = 1.0f)
    {
        return baseYieldPerCycle * laserCount * skillMultiplier;
    }

    /// <summary>
    /// Calculate yield per minute given cycle time and yield per cycle.
    /// </summary>
    public static float CalculateYieldPerMinute(float yieldPerCycle, float cycleTimeSeconds)
    {
        if (cycleTimeSeconds <= 0) return 0;
        return yieldPerCycle * (60f / cycleTimeSeconds);
    }

    /// <summary>
    /// Calculate ISK value of mined ore.
    /// </summary>
    public static float CalculateOreValue(string oreId, int quantity)
    {
        if (!OreDatabase.Ores.TryGetValue(oreId, out var def)) return 0;
        return quantity * def.BaseValue;
    }

    /// <summary>
    /// Calculate the estimated mineral output from reprocessing.
    /// </summary>
    public static System.Collections.Generic.Dictionary<string, int> EstimateReprocessingOutput(
        string oreId, int quantity, float efficiency)
    {
        var output = new System.Collections.Generic.Dictionary<string, int>();
        if (!OreDatabase.Ores.TryGetValue(oreId, out var oreDef)) return output;

        int batches = quantity / oreDef.ReprocessBatchSize;
        foreach (var (mineralId, yieldPerBatch) in oreDef.MineralYield)
        {
            int mineralYield = (int)(yieldPerBatch * batches * efficiency);
            if (mineralYield > 0)
                output[mineralId] = mineralYield;
        }
        return output;
    }

    /// <summary>
    /// Calculate estimated ISK value of reprocessing output.
    /// </summary>
    public static float EstimateReprocessingValue(string oreId, int quantity, float efficiency)
    {
        var minerals = EstimateReprocessingOutput(oreId, quantity, efficiency);
        float total = 0;
        foreach (var (mineralId, qty) in minerals)
        {
            if (OreDatabase.Minerals.TryGetValue(mineralId, out var mDef))
                total += qty * mDef.BaseValue;
        }
        return total;
    }
}
