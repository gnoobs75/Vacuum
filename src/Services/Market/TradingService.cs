using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Services.Market;

/// <summary>
/// WO-173: High-level trading operations — trade route evaluation, hauling contracts, cargo transport.
/// </summary>
public partial class TradingService : Node
{
    public static TradingService? Instance { get; private set; }

    private readonly List<HaulingContractData> _contracts = new();

    [Signal] public delegate void ContractAcceptedEventHandler(string contractId);
    [Signal] public delegate void ContractCompletedEventHandler(string contractId, double reward);
    [Signal] public delegate void TradeCompletedEventHandler(string itemId, int quantity, double profit);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[TradingService] Ready.");
    }

    public HaulingContractData CreateHaulingContract(string issuer, string origin, string destination,
        string itemTypeId, int quantity, double reward, double collateral)
    {
        var contract = new HaulingContractData
        {
            IssuerId = issuer,
            OriginStationId = origin,
            DestinationStationId = destination,
            ItemTypeId = itemTypeId,
            Quantity = quantity,
            Reward = reward,
            Collateral = collateral,
            Status = HaulingContractStatus.Open,
            ExpiresAt = DateTime.UtcNow.AddDays(3)
        };
        _contracts.Add(contract);
        return contract;
    }

    public bool AcceptContract(string contractId, string acceptor)
    {
        var contract = _contracts.FirstOrDefault(c => c.ContractId == contractId);
        if (contract == null || contract.Status != HaulingContractStatus.Open) return false;

        contract.AcceptorId = acceptor;
        contract.Status = HaulingContractStatus.Accepted;
        EmitSignal(SignalName.ContractAccepted, contractId);
        return true;
    }

    public bool StartContract(string contractId)
    {
        var contract = _contracts.FirstOrDefault(c => c.ContractId == contractId);
        if (contract == null || contract.Status != HaulingContractStatus.Accepted) return false;

        contract.Status = HaulingContractStatus.InProgress;
        return true;
    }

    public bool CompleteContract(string contractId)
    {
        var contract = _contracts.FirstOrDefault(c => c.ContractId == contractId);
        if (contract == null || contract.Status != HaulingContractStatus.InProgress) return false;

        contract.Status = HaulingContractStatus.Completed;
        EmitSignal(SignalName.ContractCompleted, contractId, contract.Reward);
        return true;
    }

    public List<HaulingContractData> GetAvailableContracts()
    {
        return _contracts.Where(c => c.Status == HaulingContractStatus.Open && c.ExpiresAt > DateTime.UtcNow).ToList();
    }

    public List<HaulingContractData> GetPlayerContracts(string playerId)
    {
        return _contracts.Where(c => c.AcceptorId == playerId).ToList();
    }

    public TradeRouteInfo EvaluateTradeRoute(string itemId, string originStation, string destStation)
    {
        var service = MarketService.Instance;
        if (service == null) return new TradeRouteInfo { ItemId = itemId };

        double buyPrice = service.GetCurrentPrice(itemId);
        double sellPrice = buyPrice * (1.0 + (destStation.GetHashCode() % 20 - 10) / 100.0);
        double margin = sellPrice - buyPrice;

        return new TradeRouteInfo
        {
            ItemId = itemId,
            Origin = originStation,
            Destination = destStation,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            ProfitMargin = margin,
            RiskLevel = CalculateRouteRisk(originStation, destStation)
        };
    }

    private static float CalculateRouteRisk(string origin, string destination)
    {
        int hash = Math.Abs((origin + destination).GetHashCode());
        return (hash % 100) / 100f;
    }

    public class TradeRouteInfo
    {
        public string ItemId { get; set; } = "";
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public double BuyPrice { get; set; }
        public double SellPrice { get; set; }
        public double ProfitMargin { get; set; }
        public float RiskLevel { get; set; }
    }
}
