using System;
using System.Threading;
using System.Threading.Tasks;
using Vacuum.Services.Mining.Config;
using Vacuum.Tasks;

namespace Vacuum.Services.Mining.Tasks;

/// <summary>
/// Background task that evaluates claim jumper spawning conditions based on
/// mining activity duration and ore value.
/// </summary>
public class ClaimJumperTask : BaseTask
{
    private readonly float _miningDuration;
    private readonly string _oreType;
    private readonly float _securityLevel;

    /// <summary>Whether the evaluation determined a jumper should spawn.</summary>
    public bool ShouldSpawn { get; private set; }
    public int RecommendedCount { get; private set; }
    public float DifficultyMultiplier { get; private set; } = 1f;

    public ClaimJumperTask(float miningDuration, string oreType, float securityLevel = 1f)
    {
        Name = "ClaimJumperEvaluation";
        _miningDuration = miningDuration;
        _oreType = oreType;
        _securityLevel = Math.Clamp(securityLevel, 0f, 1f);
    }

    public override Task<TaskResult> ExecuteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReportProgress(0.2f);

        // Must have been mining long enough
        if (_miningDuration < MiningConfig.ClaimJumperMinMiningTime)
        {
            ShouldSpawn = false;
            ReportProgress(1f);
            return Task.FromResult(TaskResult.Ok("Too early for claim jumpers"));
        }

        ReportProgress(0.5f);

        // Calculate spawn chance: base + time factor + security factor (lower sec = more danger)
        float timeFactor = (_miningDuration / 120f) * 0.1f;
        float securityFactor = (1f - _securityLevel) * 0.2f;
        float totalChance = MiningConfig.ClaimJumperBaseSpawnChance + timeFactor + securityFactor;

        // Use deterministic random based on inputs for background thread safety
        var rng = new Random((int)(_miningDuration * 1000) ^ _oreType.GetHashCode());
        ShouldSpawn = rng.NextDouble() < totalChance;

        // Difficulty scales with lower security and higher value ores
        DifficultyMultiplier = 1f + (1f - _securityLevel) * 0.5f;
        RecommendedCount = ShouldSpawn ? Math.Clamp((int)(DifficultyMultiplier * 2), 1, MiningConfig.ClaimJumperMaxActive) : 0;

        ReportProgress(1f);
        return Task.FromResult(TaskResult.Ok(
            ShouldSpawn
                ? $"Claim jumper spawn triggered: {RecommendedCount} hostiles at {DifficultyMultiplier:F1}x difficulty"
                : "No claim jumper spawn this check"));
    }
}
