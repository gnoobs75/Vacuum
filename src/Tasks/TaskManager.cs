using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Vacuum.Tasks;

/// <summary>
/// Singleton task manager using C# async/await for background processing.
/// Integrates with Godot's main thread via CallDeferred.
/// </summary>
public partial class TaskManager : Node
{
    public static TaskManager? Instance { get; private set; }

    private readonly ConcurrentDictionary<string, BaseTask> _tasks = new();
    private readonly TaskQueue _queue = new();
    private readonly ConcurrentQueue<Action> _mainThreadCallbacks = new();
    private CancellationTokenSource _cts = new();
    private int _maxConcurrent = 4;
    private int _running;

    [Signal] public delegate void TaskCompletedEventHandler(string taskId, bool success);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[TaskManager] Ready.");
    }

    public override void _Process(double delta)
    {
        // Drain main-thread callbacks
        while (_mainThreadCallbacks.TryDequeue(out var action))
        {
            try { action(); }
            catch (Exception ex) { GD.PrintErr($"[TaskManager] Callback error: {ex.Message}"); }
        }

        // Start queued tasks if capacity available
        while (_running < _maxConcurrent && _queue.TryDequeue(out var task))
        {
            if (task != null)
                _ = RunTaskAsync(task);
        }
    }

    /// <summary>Submit a task for background execution.</summary>
    public string Submit(BaseTask task)
    {
        _tasks[task.Id] = task;
        _queue.Enqueue(task);
        return task.Id;
    }

    /// <summary>Get a task by ID.</summary>
    public BaseTask? GetTask(string taskId)
    {
        return _tasks.TryGetValue(taskId, out var task) ? task : null;
    }

    private async Task RunTaskAsync(BaseTask task)
    {
        Interlocked.Increment(ref _running);
        task.Status = BaseTask.TaskStatus.Running;
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await Task.Run(() => task.ExecuteAsync(_cts.Token), _cts.Token);
            sw.Stop();
            result.Duration = sw.Elapsed;
            task.Result = result;
            task.Status = result.Success ? BaseTask.TaskStatus.Completed : BaseTask.TaskStatus.Failed;
            task.CompletedAt = DateTime.UtcNow;

            _mainThreadCallbacks.Enqueue(() =>
                EmitSignal(SignalName.TaskCompleted, task.Id, result.Success));
        }
        catch (OperationCanceledException)
        {
            task.Status = BaseTask.TaskStatus.Cancelled;
        }
        catch (Exception ex)
        {
            task.Result = TaskResult.Fail(ex.Message);
            task.Status = BaseTask.TaskStatus.Failed;
            task.CompletedAt = DateTime.UtcNow;
            GD.PrintErr($"[TaskManager] Task '{task.Name}' failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Decrement(ref _running);
        }
    }

    public override void _ExitTree()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
