namespace Vacuum.Services.Mining.Config;

/// <summary>
/// Centralized mining system configuration for balancing and adjustment.
/// </summary>
public static class MiningConfig
{
    // Asteroid Generation
    public static int DefaultAsteroidCount { get; set; } = 50;
    public static float DefaultBeltRadius { get; set; } = 500f;
    public static float AsteroidSpreadX { get; set; } = 200f;
    public static float AsteroidSpreadY { get; set; } = 30f;
    public static float AsteroidSpreadZ { get; set; } = 200f;
    public static float AsteroidRespawnTimeSeconds { get; set; } = 300f;

    // Mining Cycle
    public static float DefaultCycleTimeSeconds { get; set; } = 5f;
    public static float DefaultYieldPerCycle { get; set; } = 20f;
    public static float DefaultLaserRange { get; set; } = 150f;
    public static int MaxLasersPerShip { get; set; } = 2;

    // Heat Management
    public static float HeatPerCycle { get; set; } = 5f;
    public static float MaxHeat { get; set; } = 100f;
    public static float HeatDissipationRate { get; set; } = 3f;
    public static float OverheatThreshold { get; set; } = 80f;

    // Claim Jumpers
    public static float ClaimJumperCheckInterval { get; set; } = 15f;
    public static float ClaimJumperBaseSpawnChance { get; set; } = 0.15f;
    public static float ClaimJumperMinMiningTime { get; set; } = 20f;
    public static int ClaimJumperMaxActive { get; set; } = 4;
    public static float ClaimJumperSpawnDistance { get; set; } = 150f;
    public static float ClaimJumperBaseHealth { get; set; } = 100f;
    public static float ClaimJumperMinSpeed { get; set; } = 30f;
    public static float ClaimJumperMaxSpeed { get; set; } = 50f;

    // Reprocessing
    public static float BaseReprocessingEfficiency { get; set; } = 0.7f;
    public static int DefaultBatchSize { get; set; } = 100;
    public static float ReprocessingTimePerBatch { get; set; } = 2f;

    // Cargo
    public static float DefaultCargoCapacity { get; set; } = 500f;

    // Performance
    public static int MaxConcurrentMiningTasks { get; set; } = 4;
    public static float TaskSchedulerInterval { get; set; } = 1f;
}
