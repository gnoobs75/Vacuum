using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data.Models;
using Vacuum.Services.Market.Config;

namespace Vacuum.Services.Market;

/// <summary>
/// WO-132: Dynamic price calculation based on supply/demand, trader activity,
/// and faction controls with price floor/ceiling mechanisms.
/// </summary>
public partial class PriceCalculationEngine : Node
{
    public static PriceCalculationEngine? Instance { get; private set; }

    private float _tickTimer;

    [Signal] public delegate void PriceTickCompletedEventHandler(int itemsUpdated);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[PriceCalculationEngine] Ready.");
    }

    public override void _Process(double delta)
    {
        _tickTimer += (float)delta;
        if (_tickTimer < MarketConfig.PriceUpdateIntervalSeconds) return;
        _tickTimer = 0f;

        int updated = RunPriceTick();
        if (updated > 0)
            EmitSignal(SignalName.PriceTickCompleted, updated);
    }

    /// <summary>Run a price update cycle across all market items.</summary>
    public int RunPriceTick()
    {
        var service = MarketService.Instance;
        if (service == null) return 0;

        int updated = 0;
        foreach (var item in service.Data.Items.GetAll())
        {
            double newPrice = CalculatePrice(item);
            if (Math.Abs(newPrice - item.LastPrice) > 0.01)
            {
                double oldPrice = item.LastPrice;
                service.UpdateItemPrice(item.ItemId, newPrice);
                MarketEventHandler.Instance?.NotifyPriceChange(item.ItemId, oldPrice, newPrice);
                updated++;
            }
        }
        return updated;
    }

    /// <summary>Calculate new price for an item based on supply/demand dynamics.</summary>
    public double CalculatePrice(MarketItemData item)
    {
        double basePrice = item.BasePrice;
        if (basePrice <= 0) return item.LastPrice;

        // Supply/demand ratio
        float sdRatio = item.DemandFactor / Math.Max(item.SupplyFactor, 0.01f);
        double sdAdjustment = basePrice * (sdRatio - 1f) * MarketConfig.SupplyDemandImpact;

        // Volatility (random small fluctuation)
        double volatility = basePrice * MarketConfig.PriceVolatility * (GD.Randf() * 2f - 1f);

        // Active market events
        float eventModifier = GetEventModifier(item.ItemId);

        double newPrice = (item.LastPrice + sdAdjustment + volatility) * eventModifier;

        // Clamp to floor/ceiling
        double floor = basePrice * MarketConfig.PriceFloorMultiplier;
        double ceiling = basePrice * MarketConfig.PriceCeilingMultiplier;
        newPrice = Math.Clamp(newPrice, floor, ceiling);

        // Smooth toward base price to prevent drift
        newPrice = newPrice * 0.95 + basePrice * sdRatio * 0.05;
        return Math.Max(0.01, Math.Round(newPrice, 2));
    }

    /// <summary>Calculate supply/demand from the order book.</summary>
    public void UpdateSupplyDemand(string itemId)
    {
        var service = MarketService.Instance;
        if (service == null) return;

        var item = service.Data.Items.GetById(itemId);
        if (item == null) return;

        var book = OrderMatchingEngine.Instance?.GetOrderBook(itemId);
        if (book == null) return;

        float buyVolume = book.BuyDepth;
        float sellVolume = book.SellDepth;
        float total = buyVolume + sellVolume;

        if (total > 0)
        {
            item.DemandFactor = 0.5f + (buyVolume / total);
            item.SupplyFactor = 0.5f + (sellVolume / total);
        }
    }

    private float GetEventModifier(string itemId)
    {
        var service = MarketService.Instance;
        if (service == null) return 1f;

        float modifier = 1f;
        foreach (var evt in service.Data.MarketEvents.GetAll())
        {
            if (!evt.IsActive) continue;
            if (evt.AffectedItemId == null || evt.AffectedItemId == itemId)
                modifier *= evt.PriceModifier;
        }
        return modifier;
    }

    /// <summary>Get price statistics for an item.</summary>
    public PriceStats GetPriceStats(string itemId, int historyDays = 30)
    {
        var service = MarketService.Instance;
        if (service == null) return new PriceStats();

        var history = service.GetPriceHistory(itemId, "", historyDays);
        if (history.Count == 0)
        {
            var item = service.GetMarketItem(itemId);
            return new PriceStats { CurrentPrice = item?.LastPrice ?? 0, AveragePrice = item?.BasePrice ?? 0 };
        }

        return new PriceStats
        {
            CurrentPrice = history[0].AvgPrice,
            AveragePrice = history.Average(h => h.AvgPrice),
            HighPrice = history.Max(h => h.HighPrice),
            LowPrice = history.Min(h => h.LowPrice),
            TotalVolume = history.Sum(h => h.Volume),
            Volatility = history.Count > 1
                ? (float)(history.Max(h => h.AvgPrice) - history.Min(h => h.AvgPrice)) / (float)history.Average(h => h.AvgPrice)
                : 0f
        };
    }

    public class PriceStats
    {
        public double CurrentPrice { get; set; }
        public double AveragePrice { get; set; }
        public double HighPrice { get; set; }
        public double LowPrice { get; set; }
        public int TotalVolume { get; set; }
        public float Volatility { get; set; }
    }
}
