using Vacuum.Data.Models;

namespace Vacuum.Data.Collections;

/// <summary>
/// Pre-configured collections for market data types.
/// </summary>
public class MarketCollections
{
    public DataCollection<MarketItemData> Items { get; } = new(i => i.ItemId);
    public DataCollection<MarketOrderData> Orders { get; } = new(o => o.OrderId);
    public DataCollection<PriceHistoryData> PriceHistory { get; } = new(h => h.HistoryId);
    public DataCollection<MarketAccessData> AccessRules { get; } = new(a => a.AccessId);
    public DataCollection<MarketTransactionData> Transactions { get; } = new(t => t.TransactionId);
    public DataCollection<HaulingContractData> HaulingContracts { get; } = new(c => c.ContractId);
    public DataCollection<AITraderProfile> AITraders { get; } = new(t => t.TraderId);
    public DataCollection<MarketEventData> MarketEvents { get; } = new(e => e.EventId);
}
