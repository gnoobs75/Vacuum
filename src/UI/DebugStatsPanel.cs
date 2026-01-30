using Godot;

namespace Vacuum.UI;

/// <summary>
/// Debug stats panel toggled with F3. Shows FPS, frame time, and rendering stats.
/// </summary>
public partial class DebugStatsPanel : PanelContainer
{
    private Label? _statsLabel;
    private double _updateTimer;
    private const double UpdateInterval = 0.25; // Update 4 times per second

    public override void _Ready()
    {
        Visible = false;

        // Build the UI
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        _statsLabel = new Label();
        _statsLabel.AddThemeColorOverride("font_color", new Color(0f, 1f, 0f));
        _statsLabel.AddThemeFontSizeOverride("font_size", 14);
        margin.AddChild(_statsLabel);

        // Style the panel
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0f, 0f, 0f, 0.75f);
        styleBox.SetCornerRadiusAll(4);
        AddThemeStyleboxOverride("panel", styleBox);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.F3)
        {
            Visible = !Visible;
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!Visible || _statsLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < UpdateInterval) return;
        _updateTimer = 0;

        double fps = Engine.GetFramesPerSecond();
        double frameTime = 1000.0 / Mathf.Max(fps, 1);

        // Rendering info from the server
        var rs = RenderingServer.Singleton;
        long drawCalls = (long)Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
        long objects = (long)Performance.GetMonitor(Performance.Monitor.RenderTotalObjectsInFrame);
        long primitives = (long)Performance.GetMonitor(Performance.Monitor.RenderTotalPrimitivesInFrame);
        long videoMemUsed = (long)Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);

        // Physics info
        long physicsBodies = (long)Performance.GetMonitor(Performance.Monitor.Physics3DActiveObjects);

        // Memory
        long staticMemory = (long)Performance.GetMonitor(Performance.Monitor.MemoryStatic);

        // Node count
        long nodeCount = (long)Performance.GetMonitor(Performance.Monitor.ObjectNodeCount);
        long orphanNodes = (long)Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount);

        // Escort count
        int escorts = 0;
        var escortMgr = GetNodeOrNull<Vacuum.Systems.Flight.EscortManager>("/root/Main/EscortManager");
        if (escortMgr != null)
            escorts = escortMgr.EscortCount;

        // Belt miner count
        int beltMiners = 0;
        var minerSpawner = GetNodeOrNull<Vacuum.Systems.Mining.AsteroidMinerSpawner>("/root/Main/AsteroidMinerSpawner");
        if (minerSpawner != null)
            beltMiners = minerSpawner.MinerCount;

        _statsLabel.Text =
            $"--- DEBUG STATS (F3) ---\n" +
            $"FPS: {fps:F0}\n" +
            $"Frame Time: {frameTime:F1} ms\n" +
            $"\n--- Rendering ---\n" +
            $"Draw Calls: {drawCalls}\n" +
            $"Objects: {objects}\n" +
            $"Primitives: {primitives:N0}\n" +
            $"Video RAM: {videoMemUsed / (1024.0 * 1024.0):F1} MB\n" +
            $"\n--- Scene ---\n" +
            $"Nodes: {nodeCount}\n" +
            $"Orphan Nodes: {orphanNodes}\n" +
            $"Physics Bodies: {physicsBodies}\n" +
            $"Escorts: {escorts}\n" +
            $"Belt Miners: {beltMiners}\n" +
            $"\n--- Memory ---\n" +
            $"Static Memory: {staticMemory / (1024.0 * 1024.0):F1} MB";
    }
}
