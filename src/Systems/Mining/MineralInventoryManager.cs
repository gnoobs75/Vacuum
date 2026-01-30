using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Data;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Manages refined mineral inventory with storage limits and value tracking.
/// </summary>
public partial class MineralInventoryManager : Node
{
    public static MineralInventoryManager? Instance { get; private set; }

    private readonly Dictionary<string, int> _minerals = new();

    [Export] public int MaxMineralTypes { get; set; } = 50;

    public IReadOnlyDictionary<string, int> Minerals => _minerals;
    public int TotalMineralCount => _minerals.Values.Sum();

    [Signal] public delegate void MineralsChangedEventHandler();

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[MineralInventoryManager] Ready.");
    }

    public void AddMineral(string mineralId, int quantity)
    {
        if (quantity <= 0) return;
        if (_minerals.ContainsKey(mineralId))
            _minerals[mineralId] += quantity;
        else
            _minerals[mineralId] = quantity;
        EmitSignal(SignalName.MineralsChanged);
    }

    public void AddMinerals(Dictionary<string, int> minerals)
    {
        foreach (var (id, qty) in minerals)
            AddMineral(id, qty);
    }

    public int RemoveMineral(string mineralId, int quantity)
    {
        if (!_minerals.TryGetValue(mineralId, out var current)) return 0;
        int removed = System.Math.Min(quantity, current);
        _minerals[mineralId] -= removed;
        if (_minerals[mineralId] <= 0) _minerals.Remove(mineralId);
        EmitSignal(SignalName.MineralsChanged);
        return removed;
    }

    public int GetQuantity(string mineralId) =>
        _minerals.TryGetValue(mineralId, out var q) ? q : 0;

    /// <summary>Calculate total ISK value of all stored minerals.</summary>
    public float GetTotalValue()
    {
        float total = 0;
        foreach (var (id, qty) in _minerals)
        {
            if (OreDatabase.Minerals.TryGetValue(id, out var def))
                total += qty * def.BaseValue;
        }
        return total;
    }

    /// <summary>Get value breakdown per mineral type.</summary>
    public Dictionary<string, float> GetValueBreakdown()
    {
        var breakdown = new Dictionary<string, float>();
        foreach (var (id, qty) in _minerals)
        {
            if (OreDatabase.Minerals.TryGetValue(id, out var def))
                breakdown[id] = qty * def.BaseValue;
        }
        return breakdown;
    }
}
