using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data.Models;
using Vacuum.Services.Market.Config;
using Vacuum.Services.Market.Tasks;
using Vacuum.Tasks;

namespace Vacuum.Services.Market;

/// <summary>
/// Coordinates market background tasks: AI trading, price dynamics, and event generation.
/// </summary>
public partial class MarketTaskScheduler : Node
{
    public static MarketTaskScheduler? Instance { get; private set; }

    private readonly List<AITraderAgent> _aiAgents = new();
    private float _aiTradeTimer;
    private float _priceTimer;
    private bool _initialized;

    public int ActiveAgentCount => _aiAgents.Count(a => a.Profile.Active && !a.IsBankrupt);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[MarketTaskScheduler] Ready.");
    }

    public override void _Process(double delta)
    {
        if (!_initialized)
        {
            InitializeAgents();
            _initialized = true;
        }

        float dt = (float)delta;

        _aiTradeTimer += dt;
        if (_aiTradeTimer >= MarketConfig.AITradeIntervalSeconds)
        {
            _aiTradeTimer = 0f;
            ScheduleAITrading();
        }

        _priceTimer += dt;
        if (_priceTimer >= MarketConfig.PriceUpdateIntervalSeconds)
        {
            _priceTimer = 0f;
            SchedulePriceDynamics();
        }
    }

    private void InitializeAgents()
    {
        string[] factions = { "federation", "empire", "republic", "state" };
        string[] names = { "Alpha Trading Co", "Beta Merchants", "Gamma Exports", "Delta Logistics",
                          "Epsilon Resources", "Zeta Commerce", "Eta Supply", "Theta Markets" };

        for (int i = 0; i < MarketConfig.DefaultAITraderCount; i++)
        {
            var profile = new AITraderProfile
            {
                Name = i < names.Length ? names[i] : $"Trader_{i}",
                FactionId = factions[i % factions.Length],
                Balance = GD.RandRange(5000, 50000),
                RiskTolerance = (float)GD.RandRange(0.2, 0.8),
                Aggression = (float)GD.RandRange(0.2, 0.8)
            };

            MarketService.Instance?.Data.AITraders.Add(profile);
            _aiAgents.Add(new AITraderAgent(profile));
        }

        GD.Print($"[MarketTaskScheduler] Initialized {_aiAgents.Count} AI traders.");
    }

    private void ScheduleAITrading()
    {
        if (TaskManager.Instance == null) return;
        var task = new AITradingTask(_aiAgents);
        TaskManager.Instance.Submit(task);
    }

    private void SchedulePriceDynamics()
    {
        if (TaskManager.Instance == null) return;
        var task = new PriceDynamicsTask();
        TaskManager.Instance.Submit(task);
    }
}
