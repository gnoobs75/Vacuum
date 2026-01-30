using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vacuum.Tasks;

/// <summary>
/// Abstract base class for background tasks with progress reporting and cancellation.
/// </summary>
public abstract class BaseTask
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; protected set; } = "";
    public float Progress { get; protected set; }
    public TaskStatus Status { get; internal set; } = TaskStatus.Pending;
    public TaskResult? Result { get; internal set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; internal set; }

    public enum TaskStatus { Pending, Running, Completed, Failed, Cancelled }

    /// <summary>Execute the task. Override in subclasses.</summary>
    public abstract Task<TaskResult> ExecuteAsync(CancellationToken ct);

    protected void ReportProgress(float progress)
    {
        Progress = Math.Clamp(progress, 0f, 1f);
    }
}
