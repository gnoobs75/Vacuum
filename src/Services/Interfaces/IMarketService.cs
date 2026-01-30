using System.Collections.Generic;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Services.Interfaces;

public interface IMarketService
{
    // Item browsing
    List<MarketItemData> GetMarketItems(MarketSearchFilter? filter = null);
    MarketItemData? GetMarketItem(string itemId);

    // Order management
    MarketOrderData PlaceBuyOrder(string characterId, string stationId, string itemTypeId, int quantity, double price);
    MarketOrderData PlaceSellOrder(string characterId, string stationId, string itemTypeId, int quantity, double price);
    List<MarketOrderData> GetPlayerOrders(string characterId);
    List<MarketOrderData> GetStationOrders(string stationId, string? itemTypeId = null);
    bool CancelOrder(string orderId);

    // Order execution
    bool FulfillOrder(string orderId, string buyerId);

    // Price data
    List<PriceHistoryData> GetPriceHistory(string itemTypeId, string stationId, int days = 30);
    double GetAveragePrice(string itemTypeId);
    double GetCurrentPrice(string itemTypeId);

    // Access control
    MarketAccessLevel GetAccessLevel(string factionId, string itemId, float standing);
}
