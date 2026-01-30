using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vacuum.Data.Models;
using Vacuum.Services.Market.Config;
using Vacuum.Tasks;

namespace Vacuum.Services.Market.Tasks;

/// <summary>
/// Background task that runs AI trader agents to generate market activity.
/// </summary>
public class AITradingTask : BaseTask
{
    private readonly List<AITraderAgent> _agents;

    public int OrdersPlaced { get; private set; }
    public int TradesMatched { get; private set; }

    public AITradingTask(List<AITraderAgent> agents)
    {
        Name = "AITrading";
        _agents = agents;
    }

    public override Task<TaskResult> ExecuteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReportProgress(0.1f);

        var allOrders = new List<MarketOrderData>();

        int agentIdx = 0;
        foreach (var agent in _agents)
        {
            ct.ThrowIfCancellationRequested();
            if (!agent.Profile.Active || agent.IsBankrupt) continue;

            var orders = agent.Decide();
            allOrders.AddRange(orders);
            agentIdx++;
            ReportProgress(0.1f + 0.5f * agentIdx / _agents.Count);
        }

        OrdersPlaced = allOrders.Count;
        ReportProgress(0.7f);

        // Submit orders to market service
        var service = MarketService.Instance;
        var engine = OrderMatchingEngine.Instance;
        if (service != null && engine != null)
        {
            foreach (var order in allOrders)
            {
                service.Data.Orders.Add(order);
                var trades = engine.SubmitOrder(order);
                TradesMatched += trades.Count;
                TransactionProcessor.ProcessTrades(trades);
            }
        }

        ReportProgress(0.9f);

        // Try to generate market events
        MarketEventGenerator.TryGenerateEvent();
        MarketEventGenerator.CleanupExpiredEvents();

        ReportProgress(1f);
        return Task.FromResult(TaskResult.Ok($"AI trading: {OrdersPlaced} orders, {TradesMatched} trades"));
    }
}
