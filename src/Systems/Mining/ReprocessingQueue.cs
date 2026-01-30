using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vacuum.Services.Mining.Config;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Batch reprocessing queue with job scheduling and progress tracking.
/// </summary>
public partial class ReprocessingQueue : Node
{
    public static ReprocessingQueue? Instance { get; private set; }

    private readonly List<ReprocessingQueueItem> _queue = new();
    private ReprocessingQueueItem? _current;
    private float _processTimer;

    public int QueueLength => _queue.Count;
    public ReprocessingQueueItem? CurrentJob => _current;
    public IReadOnlyList<ReprocessingQueueItem> PendingJobs => _queue;

    [Signal] public delegate void JobStartedEventHandler(string oreId, int quantity);
    [Signal] public delegate void JobCompletedEventHandler(string oreId, int mineralsProduced);
    [Signal] public delegate void QueueEmptyEventHandler();

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[ReprocessingQueue] Ready.");
    }

    public override void _Process(double delta)
    {
        if (_current == null)
        {
            if (_queue.Count > 0)
            {
                _current = _queue[0];
                _queue.RemoveAt(0);
                _current.Status = QueueItemStatus.Processing;
                _current.StartedAt = DateTime.UtcNow;
                _processTimer = 0f;
                EmitSignal(SignalName.JobStarted, _current.OreId, _current.Quantity);
            }
            return;
        }

        _processTimer += (float)delta;
        float totalTime = _current.EstimatedTime;
        _current.Progress = totalTime > 0 ? Math.Clamp(_processTimer / totalTime, 0f, 1f) : 1f;

        if (_processTimer >= totalTime)
        {
            CompleteCurrentJob();
        }
    }

    /// <summary>Add a reprocessing job to the queue.</summary>
    public void Enqueue(string oreId, int quantity, float efficiency)
    {
        int batchCount = quantity / MiningConfig.DefaultBatchSize;
        if (batchCount <= 0) return;

        var item = new ReprocessingQueueItem
        {
            OreId = oreId,
            Quantity = quantity,
            Efficiency = efficiency,
            EstimatedTime = batchCount * MiningConfig.ReprocessingTimePerBatch,
            Status = QueueItemStatus.Queued
        };
        _queue.Add(item);
    }

    /// <summary>Remove a pending job from the queue.</summary>
    public bool Cancel(int index)
    {
        if (index < 0 || index >= _queue.Count) return false;
        _queue.RemoveAt(index);
        return true;
    }

    /// <summary>Cancel the currently processing job.</summary>
    public void CancelCurrent()
    {
        if (_current == null) return;
        _current.Status = QueueItemStatus.Cancelled;
        _current = null;
    }

    private void CompleteCurrentJob()
    {
        if (_current == null) return;

        var result = OreReprocessingSystem.Instance?.ReprocessOre(
            _current.OreId, _current.Quantity, _current.Efficiency);

        int totalMinerals = result?.Values.Sum() ?? 0;

        // Add to mineral inventory
        if (result != null)
            MineralInventoryManager.Instance?.AddMinerals(result);

        _current.Status = QueueItemStatus.Completed;
        _current.CompletedAt = DateTime.UtcNow;
        EmitSignal(SignalName.JobCompleted, _current.OreId, totalMinerals);

        _current = null;

        if (_queue.Count == 0)
            EmitSignal(SignalName.QueueEmpty);
    }

    public enum QueueItemStatus { Queued, Processing, Completed, Cancelled }

    public class ReprocessingQueueItem
    {
        public string OreId { get; set; } = "";
        public int Quantity { get; set; }
        public float Efficiency { get; set; }
        public float EstimatedTime { get; set; }
        public float Progress { get; set; }
        public QueueItemStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
