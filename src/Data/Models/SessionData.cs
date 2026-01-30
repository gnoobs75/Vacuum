using System;
using System.Collections.Generic;

namespace Vacuum.Data.Models;

/// <summary>
/// Active game session state for persistence and recovery.
/// </summary>
public class SessionData
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string PlayerId { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public string CurrentSystemId { get; set; } = "sol";
    public string? CurrentShipId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
    public List<string> ActiveTransactions { get; set; } = new();
}
