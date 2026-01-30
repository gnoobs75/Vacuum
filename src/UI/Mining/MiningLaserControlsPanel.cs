using Godot;
using Vacuum.Data;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining;

/// <summary>
/// WO-64: Mining laser controls panel with activation controls, cycle progress,
/// heat indicators, and efficiency display.
/// </summary>
public partial class MiningLaserControlsPanel : PanelContainer
{
    [Export] public NodePath MiningLaserPath { get; set; } = "";
    [Export] public NodePath TargetingPath { get; set; } = "";

    private MiningLaserSystem? _laser;
    private TargetingSystem? _targeting;
    private Label? _statusLabel;
    private ProgressBar? _cycleBar;
    private ProgressBar? _heatBar;
    private double _updateTimer;

    public override void _Ready()
    {
        Visible = false;
        SetupStyle();
        SetupUI();
        CallDeferred(MethodName.ConnectNodes);
    }

    private void SetupStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.02f, 0.06f, 0.12f, 0.85f);
        style.SetCornerRadiusAll(4);
        style.BorderColor = new Color(0.1f, 0.5f, 0.7f, 0.5f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);
    }

    private void SetupUI()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_top", 6);
        margin.AddThemeConstantOverride("margin_right", 8);
        margin.AddThemeConstantOverride("margin_bottom", 6);
        margin.AddChild(vbox);
        AddChild(margin);

        var title = new Label { Text = "LASER CONTROLS" };
        title.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 1f));
        title.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(title);

        _statusLabel = new Label();
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 1f));
        _statusLabel.AddThemeFontSizeOverride("font_size", 11);
        vbox.AddChild(_statusLabel);

        vbox.AddChild(new Label { Text = "Cycle:" });
        _cycleBar = new ProgressBar { MinValue = 0, MaxValue = 100, CustomMinimumSize = new Vector2(180, 14) };
        _cycleBar.AddThemeStyleboxOverride("fill", CreateBarStyle(new Color(0.2f, 0.7f, 1f)));
        vbox.AddChild(_cycleBar);

        vbox.AddChild(new Label { Text = "Heat:" });
        _heatBar = new ProgressBar { MinValue = 0, MaxValue = 100, CustomMinimumSize = new Vector2(180, 14) };
        _heatBar.AddThemeStyleboxOverride("fill", CreateBarStyle(new Color(1f, 0.4f, 0.1f)));
        vbox.AddChild(_heatBar);
    }

    private StyleBoxFlat CreateBarStyle(Color color)
    {
        var s = new StyleBoxFlat();
        s.BgColor = color;
        s.SetCornerRadiusAll(2);
        return s;
    }

    private void ConnectNodes()
    {
        if (!string.IsNullOrEmpty(MiningLaserPath))
            _laser = GetNodeOrNull<MiningLaserSystem>(MiningLaserPath);
        if (!string.IsNullOrEmpty(TargetingPath))
            _targeting = GetNodeOrNull<TargetingSystem>(TargetingPath);
    }

    public override void _Process(double delta)
    {
        bool show = _laser != null && (_laser.IsActive || _targeting?.HasTarget == true);
        Visible = show;
        if (!show) return;

        _updateTimer += delta;
        if (_updateTimer < 0.1) return;
        _updateTimer = 0;

        if (_laser == null || _statusLabel == null) return;

        if (_laser.IsActive)
        {
            string oreName = _laser.CurrentOreId ?? "Unknown";
            if (_laser.CurrentOreId != null && OreDatabase.Ores.TryGetValue(_laser.CurrentOreId, out var d))
                oreName = d.Name;
            _statusLabel.Text = $"MINING: {oreName}\nLasers: {_laser.ActiveLaserCount}\nYield/min: {_laser.YieldPerMinute:F0}";
        }
        else
        {
            _statusLabel.Text = "IDLE (F1 to activate)";
        }

        if (_cycleBar != null) _cycleBar.Value = _laser.CyclePercent;
        // Heat is not directly exposed, so show 0 for now
        if (_heatBar != null) _heatBar.Value = 0;
    }
}
