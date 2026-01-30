using System.Collections.Generic;
using Godot;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Mining notification system for capacity limits, interruptions, and status changes.
/// </summary>
public partial class MiningAlertsSystem : VBoxContainer
{
    private readonly List<AlertEntry> _alerts = new();
    private const int MaxAlerts = 5;
    private const float AlertDuration = 5f;

    public override void _Ready()
    {
        AnchorTop = 0;
        AnchorBottom = 0;
        AnchorLeft = 0.5f;
        AnchorRight = 0.5f;
        GrowHorizontal = GrowDirection.Both;
        AddThemeConstantOverride("separation", 2);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        for (int i = _alerts.Count - 1; i >= 0; i--)
        {
            _alerts[i].TimeRemaining -= dt;
            if (_alerts[i].TimeRemaining <= 0)
            {
                _alerts[i].Label.QueueFree();
                _alerts.RemoveAt(i);
            }
            else if (_alerts[i].TimeRemaining < 1f && _alerts[i].Label != null)
            {
                _alerts[i].Label.Modulate = new Color(1, 1, 1, _alerts[i].TimeRemaining);
            }
        }
    }

    /// <summary>Show an alert message.</summary>
    public void ShowAlert(string message, AlertType type = AlertType.Info)
    {
        if (_alerts.Count >= MaxAlerts)
        {
            _alerts[0].Label.QueueFree();
            _alerts.RemoveAt(0);
        }

        var label = new Label { Text = message, HorizontalAlignment = HorizontalAlignment.Center };
        var color = type switch
        {
            AlertType.Warning => new Color(1f, 0.8f, 0.2f),
            AlertType.Danger => new Color(1f, 0.3f, 0.2f),
            AlertType.Success => new Color(0.3f, 1f, 0.4f),
            _ => new Color(0.7f, 0.9f, 1f),
        };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", 13);
        AddChild(label);

        _alerts.Add(new AlertEntry { Label = label, TimeRemaining = AlertDuration });
    }

    public enum AlertType { Info, Warning, Danger, Success }

    private class AlertEntry
    {
        public Label Label = null!;
        public float TimeRemaining;
    }
}
