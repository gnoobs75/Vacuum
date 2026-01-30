using Godot;
using System.Collections.Generic;
using System.Linq;
using Vacuum.Data.Models;

namespace Vacuum.UI.Market.Components;

/// <summary>
/// Searchable, sortable grid of market items.
/// </summary>
public partial class MarketItemGrid : VBoxContainer
{
    private Label? _gridLabel;
    private List<MarketItemData> _items = new();

    public override void _Ready()
    {
        var title = new Label { Text = "ITEMS" };
        title.AddThemeColorOverride("font_color", new Color(0.4f, 0.7f, 1f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _gridLabel = new Label();
        _gridLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.85f, 1f));
        _gridLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_gridLabel);
    }

    public void UpdateItems(List<MarketItemData> items)
    {
        _items = items;
        if (_gridLabel == null) return;

        string text = "";
        foreach (var item in _items.Take(20))
            text += $"{item.Name}: {item.LastPrice:F1} ISK (vol: {item.VolumeTraded})\n";

        _gridLabel.Text = text.Length > 0 ? text : "(no items)";
    }
}
