using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services.Communication;

/// <summary>
/// Routes named player/system actions to registered handlers.
/// Provides a string-based dispatch for input actions, UI commands, etc.
/// </summary>
public partial class ActionRouter : Node
{
    public static ActionRouter? Instance { get; private set; }

    private readonly Dictionary<string, List<Action<object?>>> _handlers = new();

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>Register a handler for a named action.</summary>
    public void Register(string actionName, Action<object?> handler)
    {
        if (!_handlers.ContainsKey(actionName))
            _handlers[actionName] = new List<Action<object?>>();
        _handlers[actionName].Add(handler);
    }

    /// <summary>Unregister a handler.</summary>
    public void Unregister(string actionName, Action<object?> handler)
    {
        if (_handlers.TryGetValue(actionName, out var list))
            list.Remove(handler);
    }

    /// <summary>Dispatch a named action to all registered handlers.</summary>
    public void Dispatch(string actionName, object? payload = null)
    {
        if (!_handlers.TryGetValue(actionName, out var list)) return;
        foreach (var handler in list)
        {
            try
            {
                handler(payload);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[ActionRouter] Error handling '{actionName}': {ex.Message}");
            }
        }
    }

    public bool HasHandlers(string actionName) =>
        _handlers.ContainsKey(actionName) && _handlers[actionName].Count > 0;
}
