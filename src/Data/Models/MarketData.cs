using System;
using System.Collections.Generic;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// Market order for buy/sell at a station.
/// </summary>
public class MarketOrderData
{
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public string ItemTypeId { get; set; } = string.Empty;
    public MarketOrderType OrderType { get; set; } = MarketOrderType.Buy;
    public int Quantity { get; set; }
    public int FilledQuantity { get; set; }
    public double Price { get; set; }
    public MarketOrderStatus Status { get; set; } = MarketOrderStatus.Active;
    public string IssuerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public bool IsBuyOrder => OrderType == MarketOrderType.Buy;
    public int RemainingQuantity => Quantity - FilledQuantity;
    public bool IsFullyFilled => FilledQuantity >= Quantity;
}

/// <summary>
/// Historical price snapshot for market analytics.
/// </summary>
public class PriceHistoryData
{
    public string HistoryId { get; set; } = Guid.NewGuid().ToString();
    public string ItemTypeId { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public double AvgPrice { get; set; }
    public double HighPrice { get; set; }
    public double LowPrice { get; set; }
    public int Volume { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Definition of a tradeable market item.
/// </summary>
public class MarketItemData
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public ItemCategory Category { get; set; } = ItemCategory.Misc;
    public double BasePrice { get; set; }
    public double LastPrice { get; set; }
    public long VolumeTraded { get; set; }
    public float SupplyFactor { get; set; } = 1f;
    public float DemandFactor { get; set; } = 1f;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Faction-based market access restriction.
/// </summary>
public class MarketAccessData
{
    public string AccessId { get; set; } = Guid.NewGuid().ToString();
    public string FactionId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public float MinStanding { get; set; }
    public MarketAccessLevel AccessLevel { get; set; } = MarketAccessLevel.Full;
}

/// <summary>
/// Record of a completed market transaction.
/// </summary>
public class MarketTransactionData
{
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
    public string BuyOrderId { get; set; } = string.Empty;
    public string SellOrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public string ItemTypeId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Price { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Market search filter state.
/// </summary>
public class MarketSearchFilter
{
    public string SearchTerm { get; set; } = string.Empty;
    public ItemCategory? Category { get; set; }
    public string Region { get; set; } = string.Empty;
    public double MinPrice { get; set; }
    public double MaxPrice { get; set; } = double.MaxValue;
    public MarketSortOption SortBy { get; set; } = MarketSortOption.Name;
    public bool Ascending { get; set; } = true;
}

/// <summary>
/// Hauling contract for cargo transport between stations.
/// </summary>
public class HaulingContractData
{
    public string ContractId { get; set; } = Guid.NewGuid().ToString();
    public string IssuerId { get; set; } = string.Empty;
    public string AcceptorId { get; set; } = string.Empty;
    public string OriginStationId { get; set; } = string.Empty;
    public string DestinationStationId { get; set; } = string.Empty;
    public string ItemTypeId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Reward { get; set; }
    public double Collateral { get; set; }
    public HaulingContractStatus Status { get; set; } = HaulingContractStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// AI trader agent profile.
/// </summary>
public class AITraderProfile
{
    public string TraderId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string FactionId { get; set; } = string.Empty;
    public double Balance { get; set; } = 10000;
    public float RiskTolerance { get; set; } = 0.5f;
    public float Aggression { get; set; } = 0.5f;
    public List<string> PreferredItems { get; set; } = new();
    public Dictionary<string, int> Inventory { get; set; } = new();
    public double TotalProfit { get; set; }
    public int TradesCompleted { get; set; }
    public bool Active { get; set; } = true;
}

/// <summary>
/// Market event that temporarily affects prices or availability.
/// </summary>
public class MarketEventData
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public string? AffectedItemId { get; set; }
    public string? AffectedFactionId { get; set; }
    public float PriceModifier { get; set; } = 1f;
    public float SupplyModifier { get; set; } = 1f;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public float DurationSeconds { get; set; } = 300f;
    public bool IsActive => DateTime.UtcNow < StartedAt.AddSeconds(DurationSeconds);
}
