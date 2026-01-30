using System;
using System.Collections.Generic;
using Godot;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Services.Market;

/// <summary>
/// Core order matching engine with price-time priority algorithm.
/// Handles partial fills and transaction creation.
/// </summary>
public partial class OrderMatchingEngine : Node
{
    public static OrderMatchingEngine? Instance { get; private set; }

    private readonly Dictionary<string, OrderBook> _orderBooks = new();

    [Signal] public delegate void TradeExecutedEventHandler(string buyOrderId, string sellOrderId, int quantity, double price);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[OrderMatchingEngine] Ready.");
    }

    public OrderBook GetOrCreateOrderBook(string itemId)
    {
        if (!_orderBooks.TryGetValue(itemId, out var book))
        {
            book = new OrderBook(itemId);
            _orderBooks[itemId] = book;
        }
        return book;
    }

    public OrderBook? GetOrderBook(string itemId) =>
        _orderBooks.TryGetValue(itemId, out var book) ? book : null;

    /// <summary>Submit an order and attempt matching.</summary>
    public List<MarketTransactionData> SubmitOrder(MarketOrderData order)
    {
        var book = GetOrCreateOrderBook(order.ItemTypeId);
        book.AddOrder(order);

        var trades = TryMatch(book);
        return trades;
    }

    /// <summary>Attempt to match buy and sell orders in the book.</summary>
    public List<MarketTransactionData> TryMatch(OrderBook book)
    {
        var trades = new List<MarketTransactionData>();

        while (book.CanMatch())
        {
            var buy = book.PeekBestBuyOrder()!;
            var sell = book.PeekBestSellOrder()!;

            int quantity = Math.Min(buy.RemainingQuantity, sell.RemainingQuantity);
            double price = sell.Price; // execute at sell price (seller's ask)

            // Fill
            buy.FilledQuantity += quantity;
            sell.FilledQuantity += quantity;

            if (buy.IsFullyFilled) { buy.Status = MarketOrderStatus.Filled; book.RemoveOrder(buy); }
            else { buy.Status = MarketOrderStatus.Partial; }

            if (sell.IsFullyFilled) { sell.Status = MarketOrderStatus.Filled; book.RemoveOrder(sell); }
            else { sell.Status = MarketOrderStatus.Partial; }

            var tx = new MarketTransactionData
            {
                BuyOrderId = buy.OrderId,
                SellOrderId = sell.OrderId,
                BuyerId = buy.CharacterId,
                SellerId = sell.CharacterId,
                ItemTypeId = book.ItemId,
                Quantity = quantity,
                Price = price
            };
            trades.Add(tx);

            // Record in service
            MarketService.Instance?.Data.Transactions.Add(tx);
            MarketService.Instance?.RecordPriceHistory(book.ItemId, buy.StationId, price, quantity);

            EmitSignal(SignalName.TradeExecuted, buy.OrderId, sell.OrderId, quantity, price);
        }

        return trades;
    }

    /// <summary>Remove expired orders across all books.</summary>
    public int CleanupExpiredOrders()
    {
        int removed = 0;
        var now = DateTime.UtcNow;

        foreach (var book in _orderBooks.Values)
        {
            var expired = new List<MarketOrderData>();
            foreach (var order in book.BuyOrders)
                if (order.ExpiresAt < now && order.Status == MarketOrderStatus.Active)
                    expired.Add(order);
            foreach (var order in book.SellOrders)
                if (order.ExpiresAt < now && order.Status == MarketOrderStatus.Active)
                    expired.Add(order);

            foreach (var order in expired)
            {
                order.Status = MarketOrderStatus.Expired;
                book.RemoveOrder(order);
                removed++;
            }
        }

        return removed;
    }
}
