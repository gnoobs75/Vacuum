using Godot;
using Vacuum.Services;

namespace Vacuum.Core;

/// <summary>
/// Coordinates application startup: ensures all services are initialized
/// in the correct order before gameplay begins.
/// </summary>
public partial class ApplicationBootstrap : Node
{
    public static ApplicationBootstrap? Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[ApplicationBootstrap] Starting service initialization...");

        // Services auto-initialize via _Ready (added as scene children or autoloads).
        // This node runs after all autoloads, so we can start managed services.
        CallDeferred(MethodName.StartServices);
    }

    private void StartServices()
    {
        ServiceManager.Instance?.StartAll();
        GD.Print("[ApplicationBootstrap] All services started.");
    }
}
