using Godot;
using Vacuum.Data;
using Vacuum.Services.Mining.Config;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Manages ore accumulation during mining operations with capacity tracking
/// and integration with CargoHold.
/// </summary>
public partial class CargoManagement : Node
{
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private CargoHold? _cargoHold;

    public float Capacity => _cargoHold?.MaxVolume ?? MiningConfig.DefaultCargoCapacity;
    public float UsedVolume => _cargoHold?.UsedVolume ?? 0;
    public float FreeVolume => _cargoHold?.FreeVolume ?? Capacity;
    public float FillPercent => _cargoHold?.FillPercent ?? 0;
    public bool IsFull => _cargoHold?.IsFull ?? false;

    [Signal] public delegate void CargoNearFullEventHandler(float fillPercent);

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    /// <summary>
    /// Attempt to store mined ore. Returns quantity actually stored.
    /// </summary>
    public int StoreOre(string oreId, int quantity)
    {
        if (_cargoHold == null) return 0;

        float volumePerUnit = 0.1f;
        string name = oreId;
        if (OreDatabase.Ores.TryGetValue(oreId, out var def))
        {
            volumePerUnit = def.Volume;
            name = def.Name;
        }

        int stored = _cargoHold.AddItem(oreId, name, quantity, volumePerUnit);

        if (FillPercent >= 90f)
            EmitSignal(SignalName.CargoNearFull, FillPercent);

        return stored;
    }

    /// <summary>Calculate how many units of an ore can still fit.</summary>
    public int CanFit(string oreId)
    {
        if (_cargoHold == null) return 0;
        float volumePerUnit = OreDatabase.Ores.TryGetValue(oreId, out var def) ? def.Volume : 0.1f;
        return volumePerUnit > 0 ? (int)(_cargoHold.FreeVolume / volumePerUnit) : 0;
    }
}
