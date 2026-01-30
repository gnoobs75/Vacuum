using System.Collections.Generic;
using Godot;
using Vacuum.Data.Models;

namespace Vacuum.Services.Market;

/// <summary>
/// Processes market transactions: balance validation, inventory transfers, and logging.
/// </summary>
public static class TransactionProcessor
{
    /// <summary>Process a completed trade between buyer and seller.</summary>
    public static bool ProcessTrade(MarketTransactionData trade)
    {
        double totalCost = trade.Quantity * trade.Price;

        // In a full implementation this would:
        // 1. Debit buyer's balance by totalCost
        // 2. Credit seller's balance by totalCost
        // 3. Transfer items from seller to buyer inventory
        // For now, just log and record
        GD.Print($"[Transaction] {trade.BuyerId} bought {trade.Quantity}x {trade.ItemTypeId} " +
                 $"from {trade.SellerId} at {trade.Price:F2} each (total: {totalCost:F2})");

        return true;
    }

    /// <summary>Process multiple trades from a matching round.</summary>
    public static int ProcessTrades(List<MarketTransactionData> trades)
    {
        int processed = 0;
        foreach (var trade in trades)
        {
            if (ProcessTrade(trade))
                processed++;
        }
        return processed;
    }

    /// <summary>Calculate total value of a set of transactions.</summary>
    public static double CalculateTotalValue(List<MarketTransactionData> trades)
    {
        double total = 0;
        foreach (var t in trades)
            total += t.Quantity * t.Price;
        return total;
    }
}
