using System;
using System.Collections.Generic;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

public class GameEventDefinition
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TriggerCondition { get; set; } = string.Empty;
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ActiveEventData
{
    public string ActiveEventId { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Active;
    public List<string> AffectedFactions { get; set; } = new();
}

public class PlayerDecisionData
{
    public string DecisionId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string ActiveEventId { get; set; } = string.Empty;
    public int DecisionOption { get; set; }
    public DateTime MadeAt { get; set; } = DateTime.UtcNow;
}

public class EventConsequenceData
{
    public string ConsequenceId { get; set; } = Guid.NewGuid().ToString();
    public string ActiveEventId { get; set; } = string.Empty;
    public string ConsequenceType { get; set; } = string.Empty;
    public string AffectedSystem { get; set; } = string.Empty;
    public float EffectValue { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}

public class EventChainData
{
    public string ChainId { get; set; } = Guid.NewGuid().ToString();
    public string ParentEventId { get; set; } = string.Empty;
    public string ChildEventId { get; set; } = string.Empty;
    public string TriggerCondition { get; set; } = string.Empty;
}
