using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Mining operation controls: start/stop, automation, and status display.
/// </summary>
public partial class MiningControlPanel : VBoxContainer
{
    private Label? _statusLabel;
    private double _updateTimer;

    public override void _Ready()
    {
        var title = new Label { Text = "MINING OPS" };
        title.AddThemeColorOverride("font_color", new Color(0.5f, 0.9f, 0.7f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _statusLabel = new Label();
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.8f));
        _statusLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_statusLabel);

        AddChild(new Label
        {
            Text = "F1: Toggle Laser\nT: Cycle Target\nF: Dock/Undock\nR: Reprocess (docked)",
        });
    }

    public override void _Process(double delta)
    {
        _updateTimer += delta;
        if (_updateTimer < 0.2) return;
        _updateTimer = 0;

        if (_statusLabel == null) return;

        var stats = MiningStatistics.Instance;
        if (stats != null)
        {
            _statusLabel.Text =
                $"Mined: {stats.TotalOreExtracted}\n" +
                $"Cycles: {stats.TotalCyclesCompleted}\n" +
                $"Rate: {stats.YieldPerMinute:F0}/min";
        }
        else
        {
            _statusLabel.Text = "No active session.";
        }
    }
}
