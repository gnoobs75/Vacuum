using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Displays reprocessing efficiency factors and optimization suggestions.
/// </summary>
public partial class EfficiencyCalculator : VBoxContainer
{
    private Label? _effLabel;

    public override void _Ready()
    {
        var title = new Label { Text = "REPROCESSING EFFICIENCY" };
        title.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _effLabel = new Label();
        _effLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.85f, 0.95f));
        _effLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_effLabel);

        UpdateDisplay(0.7f, 0f, 0f);
    }

    public void UpdateDisplay(float baseEff, float skillBonus = 0f, float facilityBonus = 0f)
    {
        var breakdown = ReprocessingEfficiencyCalculator.GetBreakdown(baseEff, skillBonus, facilityBonus);
        if (_effLabel == null) return;

        _effLabel.Text =
            $"Base Efficiency: {breakdown.BaseEfficiency * 100:F0}%\n" +
            $"Skill Bonus: +{breakdown.SkillBonus * 100:F0}%\n" +
            $"Facility Bonus: +{breakdown.FacilityBonus * 100:F0}%\n" +
            $"Total: {breakdown.TotalEfficiency * 100:F1}%\n\n" +
            "Tip: Higher skills = more minerals.";
    }
}
