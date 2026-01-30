using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services.Communication;

/// <summary>
/// Generic event bus for typed service-to-service communication.
/// Complements GameEventBus (Godot signals) with C# delegate-based events
/// for internal service coordination.
/// </summary>
public partial class EventBus : Node
{
    public static EventBus? Instance { get; private set; }

    private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();
    private readonly object _lock = new();

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[EventBus] Ready.");
    }

    /// <summary>Subscribe to an event type.</summary>
    public void Subscribe<T>(Action<T> handler)
    {
        lock (_lock)
        {
            var type = typeof(T);
            if (!_subscriptions.ContainsKey(type))
                _subscriptions[type] = new List<Delegate>();
            _subscriptions[type].Add(handler);
        }
    }

    /// <summary>Unsubscribe from an event type.</summary>
    public void Unsubscribe<T>(Action<T> handler)
    {
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }
    }

    /// <summary>Publish an event to all subscribers.</summary>
    public void Publish<T>(T evt)
    {
        List<Delegate>? handlers;
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(typeof(T), out handlers))
                return;
            handlers = new List<Delegate>(handlers); // snapshot
        }

        foreach (var handler in handlers)
        {
            try
            {
                ((Action<T>)handler)(evt);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventBus] Handler error for {typeof(T).Name}: {ex.Message}");
            }
        }
    }

    /// <summary>Remove all subscriptions for a given type.</summary>
    public void ClearSubscriptions<T>()
    {
        lock (_lock)
        {
            _subscriptions.Remove(typeof(T));
        }
    }
}
