using System;
using System.Collections.Generic;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

public class RecruitData
{
    public string RecruitId { get; set; } = Guid.NewGuid().ToString();
    public string FactionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string BonusType { get; set; } = string.Empty;
    public float BonusValue { get; set; }
    public int RecruitmentCost { get; set; }
    public bool Availability { get; set; } = true;
}

public class CorporationMemberFullData
{
    public string MemberId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string RecruitId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public float Morale { get; set; } = 1.0f;
    public float Effectiveness { get; set; } = 1.0f;
    public DateTime HiredAt { get; set; } = DateTime.UtcNow;
    public float Loyalty { get; set; } = 1.0f;
}

public class MoraleHistoryData
{
    public string HistoryId { get; set; } = Guid.NewGuid().ToString();
    public string MemberId { get; set; } = string.Empty;
    public float MoraleChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class FleetOperationData
{
    public string OperationId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public List<string> Members { get; set; } = new();
    public FleetOperationStatus Status { get; set; } = FleetOperationStatus.Planned;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool Success { get; set; }
}

public class BetrayalEventData
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string MemberId { get; set; } = string.Empty;
    public BetrayalSeverity Severity { get; set; } = BetrayalSeverity.Minor;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Resolved { get; set; }
}
