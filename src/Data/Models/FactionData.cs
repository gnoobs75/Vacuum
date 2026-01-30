using System;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-125: Faction definition.
/// </summary>
public class FactionData
{
    public string FactionId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// WO-125: Character standing with a faction. One record per character-faction pair.
/// </summary>
public class FactionStandingData
{
    public string StandingId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string FactionId { get; set; } = string.Empty;
    public float StandingValue { get; set; } = 0.0f;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
