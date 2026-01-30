using System;
using Vacuum.Data;
using Vacuum.Services.Mining.Config;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Calculates claim jumper probabilities and risk levels for mining locations.
/// </summary>
public static class MiningRiskAssessment
{
    public enum RiskLevel { Minimal, Low, Moderate, High, Extreme }

    /// <summary>
    /// Assess risk at a given position based on distance from center (security proxy)
    /// and ore value.
    /// </summary>
    public static RiskAssessmentResult Assess(float distanceFromStar, float maxDistance, string oreType)
    {
        float securityLevel = Math.Clamp(1f - (distanceFromStar / maxDistance), 0f, 1f);

        float oreValueFactor = 0f;
        if (OreDatabase.Ores.TryGetValue(oreType, out var def))
            oreValueFactor = Math.Clamp(def.BaseValue / 500f, 0f, 1f);

        float riskScore = (1f - securityLevel) * 0.6f + oreValueFactor * 0.4f;

        float spawnChance = MiningConfig.ClaimJumperBaseSpawnChance + riskScore * 0.3f;
        float estimatedDifficulty = 1f + (1f - securityLevel) * 0.5f;

        var level = riskScore switch
        {
            < 0.15f => RiskLevel.Minimal,
            < 0.35f => RiskLevel.Low,
            < 0.55f => RiskLevel.Moderate,
            < 0.75f => RiskLevel.High,
            _ => RiskLevel.Extreme
        };

        return new RiskAssessmentResult
        {
            SecurityLevel = securityLevel,
            RiskScore = riskScore,
            Level = level,
            SpawnChancePerCheck = spawnChance,
            EstimatedDifficulty = estimatedDifficulty,
            RecommendedShipClass = level >= RiskLevel.High ? "Cruiser or better" : "Frigate+"
        };
    }

    public class RiskAssessmentResult
    {
        public float SecurityLevel { get; set; }
        public float RiskScore { get; set; }
        public RiskLevel Level { get; set; }
        public float SpawnChancePerCheck { get; set; }
        public float EstimatedDifficulty { get; set; }
        public string RecommendedShipClass { get; set; } = "";
    }
}
