using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;
using Vacuum.Services.Market.Config;

namespace Vacuum.Services.Market;

/// <summary>
/// WO-123: AI trader agent with configurable behavior patterns and decision-making.
/// </summary>
public class AITraderAgent
{
    public AITraderProfile Profile { get; }

    public AITraderAgent(AITraderProfile profile) => Profile = profile;

    /// <summary>Make trading decisions and place orders.</summary>
    public List<MarketOrderData> Decide()
    {
        var orders = new List<MarketOrderData>();
        var service = MarketService.Instance;
        if (service == null || !Profile.Active) return orders;

        var items = service.GetMarketItems();
        if (items.Count == 0) return orders;

        // Pick items to trade (prefer preferred items, or random)
        var candidates = Profile.PreferredItems.Count > 0
            ? items.Where(i => Profile.PreferredItems.Contains(i.ItemId)).ToList()
            : items;

        if (candidates.Count == 0) candidates = items;

        foreach (var item in candidates.Take(3))
        {
            var action = DecideAction(item);
            if (action == null) continue;
            orders.Add(action);
        }

        return orders;
    }

    private MarketOrderData? DecideAction(MarketItemData item)
    {
        double currentPrice = item.LastPrice;
        if (currentPrice <= 0) return null;

        // Decision based on supply/demand and agent personality
        float buySignal = item.DemandFactor > item.SupplyFactor ? 0.6f : 0.3f;
        buySignal += (Profile.Aggression - 0.5f) * 0.2f;

        bool shouldBuy = GD.Randf() < buySignal;

        int quantity = (int)GD.RandRange(MarketConfig.AIMinQuantity, MarketConfig.AIMaxQuantity);
        float spread = MarketConfig.AISpreadFactor * (1f + Profile.RiskTolerance);

        if (shouldBuy)
        {
            double bidPrice = currentPrice * (1f - spread * GD.Randf());
            double totalCost = bidPrice * quantity;
            if (totalCost > Profile.Balance * 0.3) // don't spend more than 30% of balance
                quantity = Math.Max(1, (int)(Profile.Balance * 0.3 / bidPrice));

            if (quantity <= 0 || bidPrice <= 0) return null;

            Profile.Balance -= quantity * bidPrice;
            return new MarketOrderData
            {
                CharacterId = Profile.TraderId,
                ItemTypeId = item.ItemId,
                OrderType = MarketOrderType.Buy,
                Quantity = quantity,
                Price = Math.Round(bidPrice, 2),
                IssuerId = Profile.TraderId,
                ExpiresAt = DateTime.UtcNow.AddDays(MarketConfig.DefaultOrderExpirationDays)
            };
        }
        else
        {
            // Sell from inventory
            int held = Profile.Inventory.TryGetValue(item.ItemId, out var h) ? h : 0;
            if (held <= 0)
            {
                // Simulate having some items for market liquidity
                held = (int)GD.RandRange(5, 30);
                Profile.Inventory[item.ItemId] = held;
            }

            quantity = Math.Min(quantity, held);
            double askPrice = currentPrice * (1f + spread * GD.Randf());

            if (quantity <= 0) return null;

            Profile.Inventory[item.ItemId] -= quantity;
            return new MarketOrderData
            {
                CharacterId = Profile.TraderId,
                ItemTypeId = item.ItemId,
                OrderType = MarketOrderType.Sell,
                Quantity = quantity,
                Price = Math.Round(askPrice, 2),
                IssuerId = Profile.TraderId,
                ExpiresAt = DateTime.UtcNow.AddDays(MarketConfig.DefaultOrderExpirationDays)
            };
        }
    }

    /// <summary>Update profit tracking after a trade.</summary>
    public void RecordTrade(double profit)
    {
        Profile.TotalProfit += profit;
        Profile.TradesCompleted++;
    }

    /// <summary>Check if the agent is bankrupt.</summary>
    public bool IsBankrupt => Profile.Balance <= 0 && Profile.Inventory.Values.Sum() == 0;
}
