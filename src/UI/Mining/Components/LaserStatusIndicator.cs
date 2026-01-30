using Godot;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Reusable status indicator for a single mining laser showing state and activity.
/// </summary>
public partial class LaserStatusIndicator : HBoxContainer
{
    public enum LaserState { Inactive, Active, Overheated, Damaged }

    private Label? _label;
    private ColorRect? _indicator;
    private LaserState _state = LaserState.Inactive;

    public LaserState State
    {
        get => _state;
        set { _state = value; UpdateVisual(); }
    }

    public int LaserIndex { get; set; }

    public override void _Ready()
    {
        _indicator = new ColorRect
        {
            CustomMinimumSize = new Vector2(10, 10),
            Color = new Color(0.3f, 0.3f, 0.3f)
        };
        AddChild(_indicator);

        _label = new Label { Text = $"Laser {LaserIndex + 1}: Inactive" };
        _label.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 1f));
        _label.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_label);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_indicator == null || _label == null) return;

        (_indicator.Color, _label.Text) = _state switch
        {
            LaserState.Active => (new Color(0.2f, 1f, 0.3f), $"Laser {LaserIndex + 1}: Active"),
            LaserState.Overheated => (new Color(1f, 0.4f, 0.1f), $"Laser {LaserIndex + 1}: Overheated"),
            LaserState.Damaged => (new Color(1f, 0.1f, 0.1f), $"Laser {LaserIndex + 1}: Damaged"),
            _ => (new Color(0.3f, 0.3f, 0.3f), $"Laser {LaserIndex + 1}: Inactive"),
        };
    }
}
