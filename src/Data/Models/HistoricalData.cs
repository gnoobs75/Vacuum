using System;

namespace Vacuum.Data.Models;

/// <summary>
/// General event log for audit trail and analytics.
/// </summary>
public class EventLogData
{
    public string LogId { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RelatedEntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tracks faction standing changes over time.
/// </summary>
public class StandingChangeData
{
    public string ChangeId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string FactionId { get; set; } = string.Empty;
    public float OldValue { get; set; }
    public float NewValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Session log for tracking play sessions.
/// </summary>
public class SessionLogData
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string PlayerId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string? LastSystem { get; set; }
}
