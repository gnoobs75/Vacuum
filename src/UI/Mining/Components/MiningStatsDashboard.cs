using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Dashboard component for comprehensive mining statistics display.
/// </summary>
public partial class MiningStatsDashboard : VBoxContainer
{
    private Label? _statsLabel;
    private double _updateTimer;

    public override void _Ready()
    {
        var title = new Label { Text = "MINING STATS" };
        title.AddThemeColorOverride("font_color", new Color(0.4f, 0.85f, 0.6f));
        title.AddThemeFontSizeOverride("font_size", 13);
        AddChild(title);

        _statsLabel = new Label();
        _statsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 0.8f));
        _statsLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_statsLabel);
    }

    public override void _Process(double delta)
    {
        _updateTimer += delta;
        if (_updateTimer < 0.5) return;
        _updateTimer = 0;

        var stats = MiningStatistics.Instance;
        if (stats == null || _statsLabel == null) return;

        _statsLabel.Text =
            $"Ore Extracted: {stats.TotalOreExtracted}\n" +
            $"Cycles: {stats.TotalCyclesCompleted}\n" +
            $"Mining Time: {stats.TotalMiningTime:F0}s\n" +
            $"Yield/min: {stats.YieldPerMinute:F1}\n" +
            $"Asteroids Depleted: {stats.TotalAsteroidsDepleted}\n" +
            $"ISK/hr: {stats.EstimatedIskPerHour:F0}";
    }
}
