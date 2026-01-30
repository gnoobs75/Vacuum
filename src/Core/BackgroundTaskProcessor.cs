using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Vacuum.Core;

/// <summary>
/// WO-70: Async task processing adapted from Celery to Godot.
/// Handles background processing for AI, market updates, events, faction politics.
/// Uses C# Tasks + a thread-safe queue to push results back to the main thread.
/// </summary>
public partial class BackgroundTaskProcessor : Node
{
    public static BackgroundTaskProcessor? Instance { get; private set; }

    public enum TaskPriority { Low, Normal, High, Critical }
    public enum TaskStatus { Queued, Running, Completed, Failed, Retrying }

    private readonly ConcurrentDictionary<string, GameTask> _activeTasks = new();
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new();
    private CancellationTokenSource _cts = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        // Drain main-thread callback queue
        while (_mainThreadQueue.TryDequeue(out var action))
        {
            try { action(); }
            catch (Exception ex) { GD.PrintErr($"[BackgroundTask] Main thread callback error: {ex.Message}"); }
        }
    }

    public string SubmitTask(string name, Func<CancellationToken, Task<object?>> work,
        TaskPriority priority = TaskPriority.Normal, int maxRetries = 3)
    {
        var task = new GameTask
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Priority = priority,
            MaxRetries = maxRetries,
            Status = TaskStatus.Queued,
            SubmittedAt = DateTime.UtcNow
        };

        _activeTasks[task.Id] = task;
        _ = ExecuteTaskAsync(task, work);
        return task.Id;
    }

    public TaskStatus GetTaskStatus(string taskId)
    {
        return _activeTasks.TryGetValue(taskId, out var task) ? task.Status : TaskStatus.Failed;
    }

    public object? GetTaskResult(string taskId)
    {
        return _activeTasks.TryGetValue(taskId, out var task) ? task.Result : null;
    }

    private async Task ExecuteTaskAsync(GameTask task, Func<CancellationToken, Task<object?>> work)
    {
        task.Status = TaskStatus.Running;
        for (int attempt = 0; attempt <= task.MaxRetries; attempt++)
        {
            try
            {
                task.Result = await Task.Run(() => work(_cts.Token), _cts.Token);
                task.Status = TaskStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;
                return;
            }
            catch (OperationCanceledException)
            {
                task.Status = TaskStatus.Failed;
                return;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[BackgroundTask] '{task.Name}' attempt {attempt + 1} failed: {ex.Message}");
                if (attempt < task.MaxRetries)
                {
                    task.Status = TaskStatus.Retrying;
                    int delay = (int)Math.Pow(2, attempt) * 100;
                    await Task.Delay(delay);
                }
                else
                {
                    task.Status = TaskStatus.Failed;
                }
            }
        }
    }

    /// <summary>
    /// Schedule a callback to run on the main Godot thread.
    /// </summary>
    public void RunOnMainThread(Action action)
    {
        _mainThreadQueue.Enqueue(action);
    }

    public override void _ExitTree()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private class GameTask
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public int MaxRetries { get; set; }
        public object? Result { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
