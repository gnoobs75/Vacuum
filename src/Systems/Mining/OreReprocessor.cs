using Godot;
using System.Collections.Generic;
using System.Linq;
using Vacuum.Data;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Mining;

/// <summary>
/// WO-166: Ore reprocessing system. When docked at a station (F key near station),
/// converts ore in cargo hold into refined minerals.
/// </summary>
public partial class OreReprocessor : Node
{
    [Export] public NodePath ShipPath { get; set; } = "";
    [Export] public NodePath CargoHoldPath { get; set; } = "";
    [Export] public float StationDockRange { get; set; } = 80f;
    [Export] public float BaseEfficiency { get; set; } = 0.7f; // 70% base reprocessing efficiency

    private Node3D? _ship;
    private CargoHold? _cargoHold;
    private bool _docked;
    private bool _reprocessingPanelOpen;

    // Mineral wallet (separate from cargo for simplicity)
    private readonly Dictionary<string, int> _mineralWallet = new();

    public bool IsDocked => _docked;
    public bool IsPanelOpen => _reprocessingPanelOpen;
    public IReadOnlyDictionary<string, int> MineralWallet => _mineralWallet;

    [Signal]
    public delegate void DockedEventHandler(string stationName);

    [Signal]
    public delegate void UndockedEventHandler();

    [Signal]
    public delegate void ReprocessingCompleteEventHandler(string oreId, int mineralCount);

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(ShipPath))
            _ship = GetNodeOrNull<Node3D>(ShipPath);
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.F)
            {
                if (_docked)
                    Undock();
                else
                    TryDock();
                GetViewport().SetInputAsHandled();
            }
            else if (_docked && key.Keycode == Key.R)
            {
                ReprocessAllOre();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void TryDock()
    {
        if (_ship == null) return;

        var station = FindNearestStation();
        if (station == null) return;

        float dist = _ship.GlobalPosition.DistanceTo(station.GlobalPosition);
        if (dist > StationDockRange) return;

        _docked = true;
        EmitSignal(SignalName.Docked, station.Name);
        GD.Print($"[OreReprocessor] Docked at {station.Name}");
    }

    private void Undock()
    {
        _docked = false;
        EmitSignal(SignalName.Undocked);
        GD.Print("[OreReprocessor] Undocked");
    }

    /// <summary>
    /// Reprocess all ore in cargo hold into minerals.
    /// </summary>
    public void ReprocessAllOre()
    {
        if (_cargoHold == null || !_docked) return;

        // Find all ore items in cargo
        var oreItems = _cargoHold.Items.Values
            .Where(i => OreDatabase.Ores.ContainsKey(i.ItemId))
            .ToList();

        if (oreItems.Count == 0)
        {
            GD.Print("[OreReprocessor] No ore to reprocess.");
            return;
        }

        int totalMinerals = 0;

        foreach (var oreItem in oreItems)
        {
            if (!OreDatabase.Ores.TryGetValue(oreItem.ItemId, out var oreDef)) continue;

            // Calculate batches
            int batches = oreItem.Quantity / oreDef.ReprocessBatchSize;
            int remainder = oreItem.Quantity % oreDef.ReprocessBatchSize;

            // Full batches yield minerals
            foreach (var (mineralId, yieldPerBatch) in oreDef.MineralYield)
            {
                int mineralYield = (int)(yieldPerBatch * batches * BaseEfficiency);
                if (mineralYield <= 0) continue;

                if (_mineralWallet.ContainsKey(mineralId))
                    _mineralWallet[mineralId] += mineralYield;
                else
                    _mineralWallet[mineralId] = mineralYield;

                totalMinerals += mineralYield;
            }

            // Remove processed ore (keep remainder)
            int processed = batches * oreDef.ReprocessBatchSize;
            _cargoHold.RemoveItem(oreItem.ItemId, processed);

            EmitSignal(SignalName.ReprocessingComplete, oreItem.ItemId, totalMinerals);
        }

        GD.Print($"[OreReprocessor] Reprocessed ore into {totalMinerals} total minerals.");
    }

    private Node3D? FindNearestStation()
    {
        if (_ship == null) return null;

        Node3D? nearest = null;
        float nearestDist = float.MaxValue;

        var solarSystem = GetTree().Root.FindChild("SolarSystem", true, false);
        if (solarSystem == null) return null;

        foreach (var child in solarSystem.GetChildren())
        {
            if (child is Node3D node && child.Name.ToString().StartsWith("Station"))
            {
                float dist = _ship.GlobalPosition.DistanceTo(node.GlobalPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = node;
                }
            }
        }

        return nearest;
    }
}
