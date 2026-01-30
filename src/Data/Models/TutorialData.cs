using System;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-102: Tutorial definition.
/// </summary>
public class TutorialData
{
    public string TutorialId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TutorialDifficulty Difficulty { get; set; } = TutorialDifficulty.Beginner;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-102: Step within a tutorial.
/// </summary>
public class TutorialStepData
{
    public string StepId { get; set; } = Guid.NewGuid().ToString();
    public string TutorialId { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Guidance { get; set; } = string.Empty;
}

/// <summary>
/// WO-102: Character's progress through a tutorial.
/// </summary>
public class TutorialProgressData
{
    public string ProgressId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string TutorialId { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    public bool Completed { get; set; } = false;
}

/// <summary>
/// WO-102: Record of tutorial completion.
/// </summary>
public class TutorialCompletionData
{
    public string CompletionId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string TutorialId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public int TimeSpentSeconds { get; set; }
}

/// <summary>
/// WO-102: Contextual help trigger.
/// </summary>
public class ContextualHelpData
{
    public string HelpId { get; set; } = Guid.NewGuid().ToString();
    public string TriggerCondition { get; set; } = string.Empty;
    public string HelpText { get; set; } = string.Empty;
    public string? RelatedTutorialId { get; set; }
}
