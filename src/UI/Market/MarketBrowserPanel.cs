using Godot;
using System.Linq;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;
using Vacuum.Services.Market;

namespace Vacuum.UI.Market;

/// <summary>
/// WO-85/139: Market browser interface for searching, filtering, and viewing market items.
/// Toggle with M key.
/// </summary>
public partial class MarketBrowserPanel : PanelContainer
{
    private Label? _contentLabel;
    private double _updateTimer;
    private bool _visible;
    private MarketSearchFilter _filter = new();
    private string? _selectedItemId;
    private int _scrollOffset;

    public override void _Ready()
    {
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.03f, 0.03f, 0.08f, 0.92f);
        style.SetCornerRadiusAll(6);
        style.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(380, 450) };
        margin.AddChild(scroll);

        _contentLabel = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
        _contentLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.85f, 1f));
        _contentLabel.AddThemeFontSizeOverride("font_size", 12);
        scroll.AddChild(_contentLabel);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.M)
            {
                _visible = !_visible;
                Visible = _visible;
                GetViewport().SetInputAsHandled();
            }
            else if (_visible)
            {
                if (key.Keycode == Key.Key1) { _filter.Category = null; _filter.SearchTerm = ""; }
                else if (key.Keycode == Key.Key2) _filter.Category = ItemCategory.Ore;
                else if (key.Keycode == Key.Key3) _filter.Category = ItemCategory.Mineral;
                else if (key.Keycode == Key.Key4) _filter.SortBy = _filter.SortBy == MarketSortOption.Price ? MarketSortOption.Name : MarketSortOption.Price;
                else if (key.Keycode == Key.Up) _scrollOffset = System.Math.Max(0, _scrollOffset - 1);
                else if (key.Keycode == Key.Down) _scrollOffset++;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_visible || _contentLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.3) return;
        _updateTimer = 0;

        var service = MarketService.Instance;
        if (service == null) { _contentLabel.Text = "Market service unavailable."; return; }

        var items = service.GetMarketItems(_filter);
        var text = "=== MARKET BROWSER ===\n";
        text += "[1]All [2]Ore [3]Mineral [4]Sort\n\n";

        if (_filter.Category.HasValue)
            text += $"Filter: {_filter.Category}\n";
        text += $"Sort: {_filter.SortBy} | Items: {items.Count}\n\n";

        text += $"{"Item",-20} {"Price",8} {"Vol",6}\n";
        text += new string('-', 36) + "\n";

        foreach (var item in items.Skip(_scrollOffset).Take(15))
        {
            string marker = item.ItemId == _selectedItemId ? ">" : " ";
            text += $"{marker}{item.Name,-19} {item.LastPrice,8:F1} {item.VolumeTraded,6}\n";
        }

        // Item detail if selected
        if (_selectedItemId != null)
        {
            var selected = service.GetMarketItem(_selectedItemId);
            if (selected != null)
            {
                text += $"\n--- {selected.Name} ---\n";
                text += $"Base: {selected.BasePrice:F1} | Current: {selected.LastPrice:F1}\n";
                text += $"Supply: {selected.SupplyFactor:F2} | Demand: {selected.DemandFactor:F2}\n";
                text += $"Volume: {selected.VolumeTraded}\n";

                var book = OrderMatchingEngine.Instance?.GetOrderBook(selected.ItemId);
                if (book != null)
                {
                    text += $"Bid: {book.BestBid:F1} | Ask: {book.BestAsk:F1}\n";
                    text += $"Buy depth: {book.BuyDepth} | Sell depth: {book.SellDepth}\n";
                }
            }
        }

        text += "\nM: Close | Arrow keys: Scroll";
        _contentLabel.Text = text;
    }
}
