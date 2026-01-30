using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services.Communication;

/// <summary>
/// Batches events for deferred processing, useful for high-frequency updates
/// (e.g., UI refresh) that shouldn't fire every frame.
/// </summary>
public partial class EventQueue : Node
{
    public static EventQueue? Instance { get; private set; }

    private readonly Queue<QueuedEvent> _queue = new();
    private readonly object _lock = new();
    private int _maxPerFrame = 50;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        int processed = 0;
        while (processed < _maxPerFrame)
        {
            QueuedEvent? evt;
            lock (_lock)
            {
                if (_queue.Count == 0) break;
                evt = _queue.Dequeue();
            }
            try
            {
                evt.Execute();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventQueue] Error processing queued event: {ex.Message}");
            }
            processed++;
        }
    }

    /// <summary>Enqueue an action for next frame processing.</summary>
    public void Enqueue(Action action, int priority = 0)
    {
        lock (_lock)
        {
            _queue.Enqueue(new QueuedEvent { Action = action, Priority = priority });
        }
    }

    public int PendingCount
    {
        get { lock (_lock) { return _queue.Count; } }
    }

    private class QueuedEvent
    {
        public Action Action { get; set; } = null!;
        public int Priority { get; set; }
        public void Execute() => Action();
    }
}
