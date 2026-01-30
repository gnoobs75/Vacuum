using System;

namespace Vacuum.Data.Models;

/// <summary>
/// Item in a character's inventory (non-cargo persistent items).
/// </summary>
public class InventoryItemData
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string ItemTypeId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string? LocationId { get; set; } // station or ship
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}
