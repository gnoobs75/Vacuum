using System.Collections.Generic;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Generates mining optimization recommendations based on current performance.
/// </summary>
public static class RecommendationEngine
{
    public static List<string> GetRecommendations()
    {
        var recs = new List<string>();
        var stats = MiningStatistics.Instance;

        if (stats == null)
        {
            recs.Add("Start mining to receive recommendations.");
            return recs;
        }

        if (stats.TotalCyclesCompleted == 0)
        {
            recs.Add("Lock a target (T) and activate lasers (F1).");
            return recs;
        }

        if (stats.AverageYieldPerCycle < 15)
            recs.Add("Low yield per cycle - upgrade mining lasers or target richer asteroids.");

        if (stats.YieldPerMinute < 50)
            recs.Add("Consider adding a second mining laser for better throughput.");

        if (stats.ClaimJumpersEncountered > 0 && stats.ClaimJumpersDefeated == 0)
            recs.Add("Mine in safer zones or improve combat capabilities.");

        var bestOre = stats.GetMostProfitableOre();
        if (bestOre != null)
            recs.Add($"Focus on {bestOre} for best ISK return.");

        if (stats.TotalMiningTime > 300 && stats.EstimatedIskPerHour < 500)
            recs.Add("Move to higher-value ore fields for better income.");

        if (recs.Count == 0)
            recs.Add("Mining performance looks good!");

        return recs;
    }
}
