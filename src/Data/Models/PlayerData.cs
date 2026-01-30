using System;
using System.Collections.Generic;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-109: Core player account. One player can have multiple characters.
/// </summary>
public class PlayerData
{
    public string PlayerId { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public List<string> CharacterIds { get; set; } = new();
}

/// <summary>
/// WO-109: Character belonging to a player. The active game identity.
/// </summary>
public class CharacterData
{
    public string CharacterId { get; set; } = Guid.NewGuid().ToString();
    public string PlayerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FactionId { get; set; } = string.Empty;
    public double IskBalance { get; set; } = 5000.0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
