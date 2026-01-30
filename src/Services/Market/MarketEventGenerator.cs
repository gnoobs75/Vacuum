using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data.Models;
using Vacuum.Services.Market.Config;

namespace Vacuum.Services.Market;

/// <summary>
/// Generates random market events that affect prices and availability.
/// </summary>
public static class MarketEventGenerator
{
    private static readonly string[] EventTemplates =
    {
        "Supply shortage of {0} drives prices up",
        "Surplus of {0} floods the market",
        "Faction conflict disrupts {0} trade routes",
        "New mining operation increases {0} supply",
        "Trade embargo affects {0} availability",
        "Pirate activity disrupts {0} shipments",
        "Tech breakthrough reduces {0} manufacturing costs",
        "Station upgrade boosts {0} demand"
    };

    /// <summary>Try to generate a random market event.</summary>
    public static MarketEventData? TryGenerateEvent()
    {
        if (GD.Randf() > MarketConfig.MarketEventChance) return null;

        var service = MarketService.Instance;
        if (service == null) return null;

        var items = service.Data.Items.GetAll().ToList();
        if (items.Count == 0) return null;

        var targetItem = items[(int)(GD.Randi() % items.Count)];
        int templateIdx = (int)(GD.Randi() % EventTemplates.Length);
        string desc = string.Format(EventTemplates[templateIdx], targetItem.Name);

        float priceModifier = (float)GD.RandRange(MarketConfig.MinPriceModifier, MarketConfig.MaxPriceModifier);
        float duration = (float)GD.RandRange(MarketConfig.MinEventDuration, MarketConfig.MaxEventDuration);

        var evt = new MarketEventData
        {
            AffectedItemId = targetItem.ItemId,
            Description = desc,
            PriceModifier = priceModifier,
            SupplyModifier = priceModifier > 1f ? 0.8f : 1.2f,
            DurationSeconds = duration
        };

        service.Data.MarketEvents.Add(evt);
        MarketEventHandler.Instance?.NotifyMarketEvent(evt);
        GD.Print($"[MarketEvent] {desc} (modifier: {priceModifier:F2}, duration: {duration:F0}s)");

        return evt;
    }

    /// <summary>Clean up expired events.</summary>
    public static int CleanupExpiredEvents()
    {
        var service = MarketService.Instance;
        if (service == null) return 0;

        var expired = service.Data.MarketEvents.Where(e => !e.IsActive).ToList();
        int count = 0;
        foreach (var evt in expired)
        {
            service.Data.MarketEvents.Remove(evt.EventId);
            count++;
        }
        return count;
    }
}
