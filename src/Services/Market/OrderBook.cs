using System.Collections.Generic;
using System.Linq;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Services.Market;

/// <summary>
/// Maintains buy/sell order queues for a single market item with price-time priority.
/// </summary>
public class OrderBook
{
    public string ItemId { get; }

    // Buy orders: highest price first (descending), then earliest time
    private readonly SortedList<OrderBookKey, MarketOrderData> _buyOrders = new(new BuyComparer());
    // Sell orders: lowest price first (ascending), then earliest time
    private readonly SortedList<OrderBookKey, MarketOrderData> _sellOrders = new(new SellComparer());

    public OrderBook(string itemId) => ItemId = itemId;

    public IReadOnlyList<MarketOrderData> BuyOrders => _buyOrders.Values.ToList();
    public IReadOnlyList<MarketOrderData> SellOrders => _sellOrders.Values.ToList();

    public double BestBid => _buyOrders.Count > 0 ? _buyOrders.Values[0].Price : 0;
    public double BestAsk => _sellOrders.Count > 0 ? _sellOrders.Values[0].Price : 0;
    public double Spread => BestAsk > 0 && BestBid > 0 ? BestAsk - BestBid : 0;

    public int BuyDepth => _buyOrders.Values.Sum(o => o.RemainingQuantity);
    public int SellDepth => _sellOrders.Values.Sum(o => o.RemainingQuantity);

    public void AddOrder(MarketOrderData order)
    {
        var key = new OrderBookKey(order.Price, order.CreatedAt.Ticks, order.OrderId);
        if (order.OrderType == MarketOrderType.Buy)
            _buyOrders[key] = order;
        else
            _sellOrders[key] = order;
    }

    public void RemoveOrder(MarketOrderData order)
    {
        var key = new OrderBookKey(order.Price, order.CreatedAt.Ticks, order.OrderId);
        if (order.OrderType == MarketOrderType.Buy)
            _buyOrders.Remove(key);
        else
            _sellOrders.Remove(key);
    }

    public MarketOrderData? PeekBestBuyOrder() => _buyOrders.Count > 0 ? _buyOrders.Values[0] : null;
    public MarketOrderData? PeekBestSellOrder() => _sellOrders.Count > 0 ? _sellOrders.Values[0] : null;

    public bool CanMatch() => BestBid > 0 && BestAsk > 0 && BestBid >= BestAsk;

    public record struct OrderBookKey(double Price, long Ticks, string OrderId);

    private class BuyComparer : IComparer<OrderBookKey>
    {
        public int Compare(OrderBookKey a, OrderBookKey b)
        {
            int priceCompare = b.Price.CompareTo(a.Price); // descending
            if (priceCompare != 0) return priceCompare;
            int timeCompare = a.Ticks.CompareTo(b.Ticks); // ascending (earliest first)
            return timeCompare != 0 ? timeCompare : string.Compare(a.OrderId, b.OrderId, System.StringComparison.Ordinal);
        }
    }

    private class SellComparer : IComparer<OrderBookKey>
    {
        public int Compare(OrderBookKey a, OrderBookKey b)
        {
            int priceCompare = a.Price.CompareTo(b.Price); // ascending
            if (priceCompare != 0) return priceCompare;
            int timeCompare = a.Ticks.CompareTo(b.Ticks); // ascending
            return timeCompare != 0 ? timeCompare : string.Compare(a.OrderId, b.OrderId, System.StringComparison.Ordinal);
        }
    }
}
