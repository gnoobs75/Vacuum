using Godot;
using System.Linq;
using Vacuum.Data;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining;

/// <summary>
/// WO-95: Yield optimization display with stats dashboard, efficiency metrics,
/// and optimization recommendations.
/// </summary>
public partial class YieldOptimizationPanel : PanelContainer
{
    private Label? _contentLabel;
    private double _updateTimer;
    private bool _visible;

    public override void _Ready()
    {
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.02f, 0.05f, 0.1f, 0.9f);
        style.SetCornerRadiusAll(6);
        style.BorderColor = new Color(0.15f, 0.5f, 0.3f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        AddChild(margin);

        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(280, 350) };
        margin.AddChild(scroll);

        _contentLabel = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
        _contentLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.95f, 0.8f));
        _contentLabel.AddThemeFontSizeOverride("font_size", 12);
        scroll.AddChild(_contentLabel);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.O)
        {
            _visible = !_visible;
            Visible = _visible;
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!_visible || _contentLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.5) return;
        _updateTimer = 0;

        var stats = MiningStatistics.Instance;
        if (stats == null) { _contentLabel.Text = "No mining data available."; return; }

        var text = "=== YIELD OPTIMIZATION ===\n\n";

        // Session stats
        text += "-- Session Statistics --\n";
        text += $"Total Ore: {stats.TotalOreExtracted}\n";
        text += $"Cycles: {stats.TotalCyclesCompleted}\n";
        text += $"Mining Time: {stats.TotalMiningTime:F0}s\n";
        text += $"Yield/min: {stats.YieldPerMinute:F1}\n";
        text += $"Avg/cycle: {stats.AverageYieldPerCycle:F1}\n";
        text += $"Est ISK/hr: {stats.EstimatedIskPerHour:F0}\n\n";

        // Per-ore breakdown
        text += "-- Ore Breakdown --\n";
        var oreBreakdown = stats.GetOreBreakdown();
        if (oreBreakdown.Count == 0)
        {
            text += "  (no ore mined yet)\n";
        }
        else
        {
            foreach (var (oreId, oreStats) in oreBreakdown.OrderByDescending(o => o.Value.Quantity))
            {
                string name = OreDatabase.Ores.TryGetValue(oreId, out var def) ? def.Name : oreId;
                text += $"  {name}: {oreStats.Quantity} ({oreStats.YieldPerMinute:F0}/min)\n";
            }
        }

        text += "\n-- Recommendations --\n";
        var bestOre = stats.GetMostProfitableOre();
        if (bestOre != null && OreDatabase.Ores.TryGetValue(bestOre, out var bestDef))
            text += $"  Best ore: {bestDef.Name}\n";

        if (stats.YieldPerMinute < 50)
            text += "  Consider upgrading mining lasers.\n";
        if (stats.TotalCyclesCompleted > 0 && stats.AverageYieldPerCycle < 15)
            text += "  Low avg yield - try rarer asteroids.\n";

        text += "\n-- Encounters --\n";
        text += $"Jumpers encountered: {stats.ClaimJumpersEncountered}\n";
        text += $"Jumpers defeated: {stats.ClaimJumpersDefeated}\n";

        _contentLabel.Text = text;
    }
}
