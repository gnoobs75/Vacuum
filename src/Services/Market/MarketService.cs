using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data.Collections;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;
using Vacuum.Services.Caching;
using Vacuum.Services.Interfaces;

namespace Vacuum.Services.Market;

/// <summary>
/// Singleton service managing all market operations: item browsing, order management,
/// price tracking, and faction access control.
/// </summary>
public partial class MarketService : Node, IMarketService
{
    public static MarketService? Instance { get; private set; }

    private readonly MarketCollections _data = new();

    [Signal] public delegate void OrderPlacedEventHandler(string orderId, string itemTypeId, bool isBuy);
    [Signal] public delegate void OrderFilledEventHandler(string orderId, int quantity, double price);
    [Signal] public delegate void OrderCancelledEventHandler(string orderId);
    [Signal] public delegate void PriceUpdatedEventHandler(string itemTypeId, double newPrice);
    [Signal] public delegate void MarketEventOccurredEventHandler(string eventId, string description);

    public MarketCollections Data => _data;

    public override void _Ready()
    {
        Instance = this;
        InitializeDefaultItems();
        GD.Print("[MarketService] Ready.");
    }

    private void InitializeDefaultItems()
    {
        // Seed market with ore and mineral items from OreDatabase
        foreach (var (id, ore) in Vacuum.Data.OreDatabase.Ores)
        {
            _data.Items.Add(new MarketItemData
            {
                ItemId = id,
                Name = ore.Name,
                Category = ItemCategory.Ore,
                BasePrice = ore.BaseValue,
                LastPrice = ore.BaseValue
            });
        }

        foreach (var (id, mineral) in Vacuum.Data.OreDatabase.Minerals)
        {
            _data.Items.Add(new MarketItemData
            {
                ItemId = id,
                Name = mineral.Name,
                Category = ItemCategory.Mineral,
                BasePrice = mineral.BaseValue,
                LastPrice = mineral.BaseValue
            });
        }
    }

    public List<MarketItemData> GetMarketItems(MarketSearchFilter? filter = null)
    {
        var items = _data.Items.GetAll().AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.SearchTerm))
                items = items.Where(i => i.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            if (filter.Category.HasValue)
                items = items.Where(i => i.Category == filter.Category.Value);
            if (filter.MinPrice > 0)
                items = items.Where(i => i.LastPrice >= filter.MinPrice);
            if (filter.MaxPrice < double.MaxValue)
                items = items.Where(i => i.LastPrice <= filter.MaxPrice);

            items = filter.SortBy switch
            {
                MarketSortOption.Price => filter.Ascending ? items.OrderBy(i => i.LastPrice) : items.OrderByDescending(i => i.LastPrice),
                MarketSortOption.Volume => filter.Ascending ? items.OrderBy(i => i.VolumeTraded) : items.OrderByDescending(i => i.VolumeTraded),
                _ => filter.Ascending ? items.OrderBy(i => i.Name) : items.OrderByDescending(i => i.Name)
            };
        }

        return items.ToList();
    }

    public MarketItemData? GetMarketItem(string itemId) => _data.Items.GetById(itemId);

    public MarketOrderData PlaceBuyOrder(string characterId, string stationId, string itemTypeId, int quantity, double price)
    {
        var order = new MarketOrderData
        {
            CharacterId = characterId,
            StationId = stationId,
            ItemTypeId = itemTypeId,
            OrderType = MarketOrderType.Buy,
            Quantity = quantity,
            Price = price,
            IssuerId = characterId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _data.Orders.Add(order);
        EmitSignal(SignalName.OrderPlaced, order.OrderId, itemTypeId, true);
        CacheManager.Instance?.InvalidateByPrefix($"market:{itemTypeId}");
        return order;
    }

    public MarketOrderData PlaceSellOrder(string characterId, string stationId, string itemTypeId, int quantity, double price)
    {
        var order = new MarketOrderData
        {
            CharacterId = characterId,
            StationId = stationId,
            ItemTypeId = itemTypeId,
            OrderType = MarketOrderType.Sell,
            Quantity = quantity,
            Price = price,
            IssuerId = characterId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _data.Orders.Add(order);
        EmitSignal(SignalName.OrderPlaced, order.OrderId, itemTypeId, false);
        CacheManager.Instance?.InvalidateByPrefix($"market:{itemTypeId}");
        return order;
    }

    public List<MarketOrderData> GetPlayerOrders(string characterId)
    {
        return _data.Orders.Where(o => o.CharacterId == characterId);
    }

    public List<MarketOrderData> GetStationOrders(string stationId, string? itemTypeId = null)
    {
        return _data.Orders.Where(o =>
            o.StationId == stationId &&
            o.Status == MarketOrderStatus.Active &&
            (itemTypeId == null || o.ItemTypeId == itemTypeId));
    }

    public bool CancelOrder(string orderId)
    {
        var order = _data.Orders.GetById(orderId);
        if (order == null || order.Status != MarketOrderStatus.Active) return false;

        order.Status = MarketOrderStatus.Cancelled;
        EmitSignal(SignalName.OrderCancelled, orderId);
        return true;
    }

    public bool FulfillOrder(string orderId, string buyerId)
    {
        var order = _data.Orders.GetById(orderId);
        if (order == null || order.Status != MarketOrderStatus.Active) return false;

        order.Status = MarketOrderStatus.Filled;
        order.FilledQuantity = order.Quantity;
        EmitSignal(SignalName.OrderFilled, orderId, order.Quantity, order.Price);
        RecordPriceHistory(order.ItemTypeId, order.StationId, order.Price, order.Quantity);
        return true;
    }

    public List<PriceHistoryData> GetPriceHistory(string itemTypeId, string stationId, int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return _data.PriceHistory.Where(h =>
            h.ItemTypeId == itemTypeId &&
            (string.IsNullOrEmpty(stationId) || h.StationId == stationId) &&
            h.Date >= cutoff)
            .OrderByDescending(h => h.Date)
            .ToList();
    }

    public double GetAveragePrice(string itemTypeId)
    {
        var item = _data.Items.GetById(itemTypeId);
        return item?.LastPrice ?? 0;
    }

    public double GetCurrentPrice(string itemTypeId)
    {
        // Check cache first
        var cached = CacheManager.Instance?.Get<PriceCacheEntry>($"market:{itemTypeId}:price");
        if (cached != null) return cached.Price;

        var item = _data.Items.GetById(itemTypeId);
        double price = item?.LastPrice ?? item?.BasePrice ?? 0;

        CacheManager.Instance?.Set($"market:{itemTypeId}:price", new PriceCacheEntry { Price = price }, 60);
        return price;
    }

    public MarketAccessLevel GetAccessLevel(string factionId, string itemId, float standing)
    {
        var rule = _data.AccessRules.FirstOrDefault(a =>
            a.FactionId == factionId && a.ItemId == itemId);
        if (rule == null) return MarketAccessLevel.Full;
        return standing >= rule.MinStanding ? rule.AccessLevel : MarketAccessLevel.Denied;
    }

    public List<PriceHistoryData> GetPriceHistory(string itemTypeId, int days = 30)
    {
        return GetPriceHistory(itemTypeId, "", days);
    }

    public List<MarketEventData> GetActiveEvents()
    {
        return _data.MarketEvents.Where(e => e.IsActive);
    }

    public void UpdateItemPrice(string itemId, double newPrice)
    {
        var item = _data.Items.GetById(itemId);
        if (item == null) return;
        item.LastPrice = newPrice;
        item.UpdatedAt = DateTime.UtcNow;
        CacheManager.Instance?.Invalidate($"market:{itemId}:price");
        EmitSignal(SignalName.PriceUpdated, itemId, newPrice);
    }

    public void RecordPriceHistory(string itemTypeId, string stationId, double price, int volume)
    {
        _data.PriceHistory.Add(new PriceHistoryData
        {
            ItemTypeId = itemTypeId,
            StationId = stationId,
            AvgPrice = price,
            HighPrice = price,
            LowPrice = price,
            Volume = volume
        });

        var item = _data.Items.GetById(itemTypeId);
        if (item != null)
        {
            item.LastPrice = price;
            item.VolumeTraded += volume;
            item.UpdatedAt = DateTime.UtcNow;
        }
    }

    private class PriceCacheEntry
    {
        public double Price { get; set; }
    }
}
