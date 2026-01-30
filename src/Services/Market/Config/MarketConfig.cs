namespace Vacuum.Services.Market.Config;

/// <summary>
/// Centralized configuration for market system parameters.
/// </summary>
public static class MarketConfig
{
    // Price Dynamics
    public static float PriceUpdateIntervalSeconds { get; set; } = 120f;
    public static float SupplyDemandImpact { get; set; } = 0.1f;
    public static float PriceVolatility { get; set; } = 0.02f;
    public static float PriceFloorMultiplier { get; set; } = 0.2f;
    public static float PriceCeilingMultiplier { get; set; } = 5f;

    // AI Trading
    public static int DefaultAITraderCount { get; set; } = 8;
    public static float AITradeIntervalSeconds { get; set; } = 60f;
    public static float AIMinOrderValue { get; set; } = 10f;
    public static float AIMaxOrderValue { get; set; } = 5000f;
    public static int AIMinQuantity { get; set; } = 1;
    public static int AIMaxQuantity { get; set; } = 100;
    public static float AISpreadFactor { get; set; } = 0.05f;

    // Market Events
    public static float MarketEventChance { get; set; } = 0.05f;
    public static float MinEventDuration { get; set; } = 60f;
    public static float MaxEventDuration { get; set; } = 600f;
    public static float MinPriceModifier { get; set; } = 0.7f;
    public static float MaxPriceModifier { get; set; } = 1.5f;

    // Orders
    public static int DefaultOrderExpirationDays { get; set; } = 7;
    public static float OrderCleanupIntervalSeconds { get; set; } = 300f;

    // Hauling
    public static float BaseHaulingRewardPerUnit { get; set; } = 1f;
    public static float HaulingCollateralPercent { get; set; } = 0.1f;
    public static float PiracyBaseChance { get; set; } = 0.05f;
}
