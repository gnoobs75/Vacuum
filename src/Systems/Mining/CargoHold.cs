using Godot;
using System.Collections.Generic;
using System.Linq;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Cargo hold attached to a ship. Stores ore and items with volume limits.
/// </summary>
public partial class CargoHold : Node
{
    [Export] public float MaxVolume { get; set; } = 500f; // m³

    private readonly Dictionary<string, CargoItem> _items = new();

    public float UsedVolume => _items.Values.Sum(i => i.TotalVolume);
    public float FreeVolume => MaxVolume - UsedVolume;
    public float FillPercent => MaxVolume > 0 ? UsedVolume / MaxVolume * 100f : 0f;
    public bool IsFull => FreeVolume <= 0.001f;
    public IReadOnlyDictionary<string, CargoItem> Items => _items;

    [Signal]
    public delegate void CargoChangedEventHandler();

    [Signal]
    public delegate void CargoFullEventHandler();

    /// <summary>
    /// Add ore/items to the hold. Returns the quantity actually added (may be less if full).
    /// </summary>
    public int AddItem(string itemId, string name, int quantity, float volumePerUnit)
    {
        float volumeNeeded = quantity * volumePerUnit;
        int canFit = quantity;

        if (volumeNeeded > FreeVolume)
        {
            canFit = (int)(FreeVolume / volumePerUnit);
            if (canFit <= 0) return 0;
        }

        if (_items.TryGetValue(itemId, out var existing))
        {
            existing.Quantity += canFit;
        }
        else
        {
            _items[itemId] = new CargoItem
            {
                ItemId = itemId,
                Name = name,
                Quantity = canFit,
                VolumePerUnit = volumePerUnit
            };
        }

        EmitSignal(SignalName.CargoChanged);
        if (IsFull) EmitSignal(SignalName.CargoFull);
        return canFit;
    }

    /// <summary>
    /// Remove items from the hold. Returns quantity actually removed.
    /// </summary>
    public int RemoveItem(string itemId, int quantity)
    {
        if (!_items.TryGetValue(itemId, out var item)) return 0;

        int removed = System.Math.Min(quantity, item.Quantity);
        item.Quantity -= removed;

        if (item.Quantity <= 0)
            _items.Remove(itemId);

        EmitSignal(SignalName.CargoChanged);
        return removed;
    }

    public int GetItemCount(string itemId)
    {
        return _items.TryGetValue(itemId, out var item) ? item.Quantity : 0;
    }

    public void Clear()
    {
        _items.Clear();
        EmitSignal(SignalName.CargoChanged);
    }
}
