using System;

namespace Vacuum.Data.Models;

/// <summary>
/// Market order for buy/sell at a station.
/// </summary>
public class MarketOrderData
{
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public string StationId { get; set; } = string.Empty;
    public string ItemTypeId { get; set; } = string.Empty;
    public bool IsBuyOrder { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public string IssuerId { get; set; } = string.Empty; // character or NPC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Historical price snapshot for market analytics.
/// </summary>
public class PriceHistoryData
{
    public string ItemTypeId { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public double AvgPrice { get; set; }
    public double HighPrice { get; set; }
    public double LowPrice { get; set; }
    public int Volume { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
