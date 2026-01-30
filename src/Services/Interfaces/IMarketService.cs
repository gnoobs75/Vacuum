using System.Collections.Generic;
using Vacuum.Data.Models;

namespace Vacuum.Services.Interfaces;

public interface IMarketService
{
    MarketOrderData PlaceBuyOrder(string characterId, string stationId, string itemTypeId, int quantity, double price);
    MarketOrderData PlaceSellOrder(string characterId, string stationId, string itemTypeId, int quantity, double price);
    bool FulfillOrder(string orderId, string buyerId);
    List<MarketOrderData> GetStationOrders(string stationId, string? itemTypeId = null);
    List<PriceHistoryData> GetPriceHistory(string itemTypeId, string stationId, int days = 30);
    double GetAveragePrice(string itemTypeId);
}
