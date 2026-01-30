using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Central service registry for dependency resolution. Services register themselves
/// on _Ready and can be retrieved by type from anywhere.
/// </summary>
public partial class ServiceLocator : Node
{
    public static ServiceLocator? Instance { get; private set; }

    private readonly Dictionary<Type, object> _services = new();

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[ServiceLocator] Ready.");
    }

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
        GD.Print($"[ServiceLocator] Registered {typeof(T).Name}");
    }

    public T? Get<T>() where T : class
    {
        return _services.TryGetValue(typeof(T), out var svc) ? (T)svc : null;
    }

    public T Require<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var svc))
            return (T)svc;
        throw new ServiceException($"Required service {typeof(T).Name} not registered.");
    }

    public bool Has<T>() where T : class => _services.ContainsKey(typeof(T));

    public void Unregister<T>() where T : class => _services.Remove(typeof(T));
}
