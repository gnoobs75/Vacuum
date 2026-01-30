using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data;
using Vacuum.Data.Models;
using Vacuum.Services.Mining.Config;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Enhanced ore reprocessing system with batch processing, efficiency calculations,
/// and configurable recipes.
/// </summary>
public partial class OreReprocessingSystem : Node
{
    public static OreReprocessingSystem? Instance { get; private set; }

    [Signal] public delegate void BatchCompletedEventHandler(string oreId, int mineralsProduced);
    [Signal] public delegate void QueueUpdatedEventHandler();

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[OreReprocessingSystem] Ready.");
    }

    /// <summary>
    /// Reprocess ore into minerals with the given efficiency.
    /// Returns a dictionary of mineralId -> quantity.
    /// </summary>
    public Dictionary<string, int> ReprocessOre(string oreId, int quantity, float efficiency)
    {
        var output = new Dictionary<string, int>();
        if (!OreDatabase.Ores.TryGetValue(oreId, out var oreDef)) return output;

        int batchSize = oreDef.ReprocessBatchSize > 0 ? oreDef.ReprocessBatchSize : MiningConfig.DefaultBatchSize;
        int batches = quantity / batchSize;
        if (batches <= 0) return output;

        foreach (var (mineralId, yieldPerBatch) in oreDef.MineralYield)
        {
            int mineralYield = (int)(yieldPerBatch * batches * efficiency);
            if (mineralYield > 0)
                output[mineralId] = mineralYield;
        }

        int totalMinerals = output.Values.Sum();
        EmitSignal(SignalName.BatchCompleted, oreId, totalMinerals);
        return output;
    }

    /// <summary>Calculate how many units of ore won't be processed (remainder).</summary>
    public int GetRemainder(string oreId, int quantity)
    {
        if (!OreDatabase.Ores.TryGetValue(oreId, out var oreDef)) return quantity;
        int batchSize = oreDef.ReprocessBatchSize > 0 ? oreDef.ReprocessBatchSize : MiningConfig.DefaultBatchSize;
        return quantity % batchSize;
    }

    /// <summary>Preview reprocessing output without actually processing.</summary>
    public Dictionary<string, int> PreviewOutput(string oreId, int quantity, float efficiency)
    {
        return Vacuum.Services.Mining.YieldCalculator.EstimateReprocessingOutput(oreId, quantity, efficiency);
    }

    /// <summary>Calculate the ISK value of reprocessing output.</summary>
    public float EstimateOutputValue(string oreId, int quantity, float efficiency)
    {
        return Vacuum.Services.Mining.YieldCalculator.EstimateReprocessingValue(oreId, quantity, efficiency);
    }

    /// <summary>Compare selling ore vs reprocessing and selling minerals.</summary>
    public (float oreValue, float mineralValue, bool reprocessIsBetter) CompareValueOptions(
        string oreId, int quantity, float efficiency)
    {
        float oreValue = Vacuum.Services.Mining.YieldCalculator.CalculateOreValue(oreId, quantity);
        float mineralValue = EstimateOutputValue(oreId, quantity, efficiency);
        return (oreValue, mineralValue, mineralValue > oreValue);
    }
}
