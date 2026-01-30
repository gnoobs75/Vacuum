using Godot;
using System.Linq;
using Vacuum.Data;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Displays available ores in cargo with selection and filtering.
/// </summary>
public partial class OreInventoryDisplay : VBoxContainer
{
    private Label? _contentLabel;
    private CargoHold? _cargoHold;
    private string? _filterOreType;
    private double _updateTimer;

    public void SetCargoHold(CargoHold hold) => _cargoHold = hold;
    public void SetFilter(string? oreType) => _filterOreType = oreType;

    public override void _Ready()
    {
        var title = new Label { Text = "ORE CARGO" };
        title.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 1f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _contentLabel = new Label();
        _contentLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 1f));
        _contentLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_contentLabel);
    }

    public override void _Process(double delta)
    {
        _updateTimer += delta;
        if (_updateTimer < 0.3) return;
        _updateTimer = 0;

        if (_cargoHold == null || _contentLabel == null) return;

        var items = _cargoHold.Items.Values
            .Where(i => OreDatabase.Ores.ContainsKey(i.ItemId))
            .Where(i => _filterOreType == null || i.ItemId == _filterOreType)
            .OrderByDescending(i => i.Quantity);

        string text = "";
        foreach (var item in items)
            text += $"{item.Name}: {item.Quantity} ({item.TotalVolume:F1}m³)\n";

        _contentLabel.Text = text.Length > 0 ? text : "(empty)";
    }
}
