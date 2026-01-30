using System.Collections.Generic;
using Godot;
using Vacuum.Services.Mining.Config;
using Vacuum.Services.Mining.Tasks;
using Vacuum.Tasks;

namespace Vacuum.Services.Mining;

/// <summary>
/// Coordinates mining-related background tasks with proper prioritization.
/// </summary>
public partial class MiningTaskScheduler : Node
{
    public static MiningTaskScheduler? Instance { get; private set; }

    private readonly List<string> _activeTaskIds = new();
    private float _schedulerTimer;

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[MiningTaskScheduler] Ready.");
    }

    public override void _Process(double delta)
    {
        _schedulerTimer += (float)delta;
        if (_schedulerTimer < MiningConfig.TaskSchedulerInterval) return;
        _schedulerTimer = 0f;

        // Clean up completed tasks
        _activeTaskIds.RemoveAll(id =>
        {
            var task = TaskManager.Instance?.GetTask(id);
            return task == null || task.Status is BaseTask.TaskStatus.Completed
                or BaseTask.TaskStatus.Failed or BaseTask.TaskStatus.Cancelled;
        });
    }

    /// <summary>Submit an asteroid generation task.</summary>
    public string? ScheduleAsteroidGeneration(string systemId, float beltRadius = 0, int count = 0)
    {
        if (TaskManager.Instance == null) return null;
        if (_activeTaskIds.Count >= MiningConfig.MaxConcurrentMiningTasks) return null;

        var task = new AsteroidGenerationTask(systemId, beltRadius, count);
        var id = TaskManager.Instance.Submit(task);
        _activeTaskIds.Add(id);
        return id;
    }

    /// <summary>Submit a claim jumper evaluation task.</summary>
    public string? ScheduleClaimJumperCheck(float miningDuration, string oreType, float securityLevel = 1f)
    {
        if (TaskManager.Instance == null) return null;

        var task = new ClaimJumperTask(miningDuration, oreType, securityLevel);
        var id = TaskManager.Instance.Submit(task);
        _activeTaskIds.Add(id);
        return id;
    }

    /// <summary>Submit a mining cycle calculation task.</summary>
    public string? ScheduleMiningCycleCalc(string operationId, string oreType, int laserCount,
        float skillMultiplier = 1f, float currentHeat = 0f)
    {
        if (TaskManager.Instance == null) return null;

        var task = new MiningCycleTask(operationId, oreType, laserCount, skillMultiplier, currentHeat);
        var id = TaskManager.Instance.Submit(task);
        _activeTaskIds.Add(id);
        return id;
    }

    public int ActiveTaskCount => _activeTaskIds.Count;
}
