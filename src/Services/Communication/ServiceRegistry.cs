using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services.Communication;

/// <summary>
/// Service registration and discovery with initialization flow tracking.
/// </summary>
public partial class ServiceRegistry : Node
{
    public static ServiceRegistry? Instance { get; private set; }

    private readonly Dictionary<string, ServiceInfo> _registry = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public void Register(string name, Node service, string[] dependencies = null!)
    {
        _registry[name] = new ServiceInfo
        {
            Name = name,
            Service = service,
            Dependencies = dependencies ?? Array.Empty<string>(),
            Registered = true
        };
    }

    public Node? Resolve(string name)
    {
        return _registry.TryGetValue(name, out var info) ? info.Service : null;
    }

    public T? Resolve<T>(string name) where T : Node
    {
        return Resolve(name) as T;
    }

    public bool IsRegistered(string name) => _registry.ContainsKey(name);

    public IReadOnlyDictionary<string, ServiceInfo> GetAll() => _registry;

    public class ServiceInfo
    {
        public string Name { get; set; } = "";
        public Node Service { get; set; } = null!;
        public string[] Dependencies { get; set; } = Array.Empty<string>();
        public bool Registered { get; set; }
    }
}
