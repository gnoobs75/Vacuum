using Godot;
using System.Linq;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Real-time cargo hold status during mining operations.
/// </summary>
public partial class CargoStatusDisplay : VBoxContainer
{
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private CargoHold? _cargoHold;
    private Label? _cargoLabel;
    private ProgressBar? _fillBar;
    private double _updateTimer;

    public override void _Ready()
    {
        var title = new Label { Text = "CARGO" };
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.4f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _fillBar = new ProgressBar
        {
            MinValue = 0, MaxValue = 100,
            CustomMinimumSize = new Vector2(140, 12)
        };
        AddChild(_fillBar);

        _cargoLabel = new Label();
        _cargoLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.85f, 0.6f));
        _cargoLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_cargoLabel);

        CallDeferred(MethodName.ConnectNodes);
    }

    private void ConnectNodes()
    {
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    public override void _Process(double delta)
    {
        _updateTimer += delta;
        if (_updateTimer < 0.2) return;
        _updateTimer = 0;

        if (_cargoHold == null || _cargoLabel == null) return;

        if (_fillBar != null) _fillBar.Value = _cargoHold.FillPercent;

        string text = $"{_cargoHold.UsedVolume:F1}/{_cargoHold.MaxVolume:F0} m³\n";
        foreach (var item in _cargoHold.Items.Values.OrderByDescending(i => i.Quantity).Take(5))
            text += $"  {item.Name}: {item.Quantity}\n";

        if (!_cargoHold.Items.Any())
            text += "  (empty)";

        _cargoLabel.Text = text;
    }
}
