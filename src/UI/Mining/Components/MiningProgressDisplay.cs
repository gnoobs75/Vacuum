using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Displays mining cycle progress, yields, and efficiency metrics.
/// </summary>
public partial class MiningProgressDisplay : VBoxContainer
{
    private Label? _cycleLabel;
    private ProgressBar? _cycleBar;
    private Label? _yieldLabel;
    private Label? _efficiencyLabel;
    private double _updateTimer;

    public MiningLaserSystem? Laser { get; set; }

    public override void _Ready()
    {
        _cycleLabel = new Label { Text = "Cycle: --" };
        _cycleLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 1f));
        _cycleLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_cycleLabel);

        _cycleBar = new ProgressBar
        {
            MinValue = 0, MaxValue = 100,
            CustomMinimumSize = new Vector2(160, 12)
        };
        AddChild(_cycleBar);

        _yieldLabel = new Label { Text = "Yield: --" };
        _yieldLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 1f));
        _yieldLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_yieldLabel);

        _efficiencyLabel = new Label { Text = "" };
        _efficiencyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 0.5f));
        _efficiencyLabel.AddThemeFontSizeOverride("font_size", 10);
        AddChild(_efficiencyLabel);
    }

    public override void _Process(double delta)
    {
        _updateTimer += delta;
        if (_updateTimer < 0.15) return;
        _updateTimer = 0;

        if (Laser == null) return;

        if (_cycleBar != null) _cycleBar.Value = Laser.CyclePercent;
        if (_cycleLabel != null) _cycleLabel.Text = Laser.IsActive ? $"Cycle: {Laser.CyclePercent:F0}%" : "Cycle: Idle";
        if (_yieldLabel != null) _yieldLabel.Text = $"Total mined: {Laser.TotalOreMined} | Cycles: {Laser.TotalCycles}";
        if (_efficiencyLabel != null) _efficiencyLabel.Text = $"Yield/min: {Laser.YieldPerMinute:F0}";
    }
}
