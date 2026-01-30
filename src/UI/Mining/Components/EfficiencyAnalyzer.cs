using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Displays mining efficiency metrics with factor breakdowns.
/// </summary>
public partial class EfficiencyAnalyzer : VBoxContainer
{
    private Label? _effLabel;

    public override void _Ready()
    {
        var title = new Label { Text = "EFFICIENCY" };
        title.AddThemeColorOverride("font_color", new Color(0.4f, 0.7f, 1f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _effLabel = new Label();
        _effLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.85f, 1f));
        _effLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_effLabel);
    }

    public void UpdateEfficiency(float baseEff, float skillBonus, float equipBonus)
    {
        var breakdown = ReprocessingEfficiencyCalculator.GetBreakdown(baseEff, skillBonus, equipBonus);
        if (_effLabel == null) return;

        _effLabel.Text =
            $"Base: {breakdown.BaseEfficiency * 100:F0}%\n" +
            $"Skills: +{breakdown.SkillBonus * 100:F0}%\n" +
            $"Equipment: +{breakdown.FacilityBonus * 100:F0}%\n" +
            $"Total: {breakdown.TotalEfficiency * 100:F1}%";
    }
}
