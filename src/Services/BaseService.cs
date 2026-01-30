using Godot;

namespace Vacuum.Services;

/// <summary>
/// Abstract base class for all game services. Provides lifecycle management,
/// logging, and standardized initialization patterns.
/// </summary>
public abstract partial class BaseService : Node
{
    public enum ServiceState { Uninitialized, Initializing, Ready, Running, Stopping, Stopped, Error }

    public ServiceState State { get; protected set; } = ServiceState.Uninitialized;
    public string ServiceName => GetType().Name;

    public override void _Ready()
    {
        State = ServiceState.Initializing;
        try
        {
            InitializeService();
            State = ServiceState.Ready;
            Log($"Initialized.");
        }
        catch (System.Exception ex)
        {
            State = ServiceState.Error;
            LogError($"Initialization failed: {ex.Message}");
        }
    }

    /// <summary>Override to perform service-specific initialization.</summary>
    protected virtual void InitializeService() { }

    /// <summary>Start the service (called after all services initialized).</summary>
    public virtual void StartService()
    {
        State = ServiceState.Running;
        Log("Started.");
    }

    /// <summary>Stop the service gracefully.</summary>
    public virtual void StopService()
    {
        State = ServiceState.Stopping;
        Log("Stopping...");
        CleanupService();
        State = ServiceState.Stopped;
    }

    /// <summary>Override to perform cleanup on shutdown.</summary>
    protected virtual void CleanupService() { }

    public override void _ExitTree()
    {
        if (State == ServiceState.Running || State == ServiceState.Ready)
            StopService();
    }

    protected void Log(string message) => GD.Print($"[{ServiceName}] {message}");
    protected void LogWarning(string message) => GD.PushWarning($"[{ServiceName}] {message}");
    protected void LogError(string message) => GD.PrintErr($"[{ServiceName}] {message}");
}
