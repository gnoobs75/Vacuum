using Godot;
using System.Linq;
using Vacuum.Services.Market;

namespace Vacuum.UI.Market;

/// <summary>
/// WO-110/147: Price history and market analytics panel. Toggle with H key.
/// </summary>
public partial class PriceHistoryPanel : PanelContainer
{
    private Label? _contentLabel;
    private double _updateTimer;
    private bool _visible;
    private string? _selectedItemId;
    private int _itemIndex;

    public override void _Ready()
    {
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.02f, 0.05f, 0.08f, 0.92f);
        style.SetCornerRadiusAll(6);
        style.BorderColor = new Color(0.2f, 0.6f, 0.4f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(360, 420) };
        margin.AddChild(scroll);

        _contentLabel = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
        _contentLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 0.8f));
        _contentLabel.AddThemeFontSizeOverride("font_size", 12);
        scroll.AddChild(_contentLabel);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.H)
            {
                _visible = !_visible;
                Visible = _visible;
                GetViewport().SetInputAsHandled();
            }
            else if (_visible)
            {
                if (key.Keycode == Key.Left) _itemIndex = System.Math.Max(0, _itemIndex - 1);
                else if (key.Keycode == Key.Right) _itemIndex++;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_visible || _contentLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.5) return;
        _updateTimer = 0;

        var service = MarketService.Instance;
        if (service == null) { _contentLabel.Text = "Market unavailable."; return; }

        var items = service.GetMarketItems(new());
        if (items.Count == 0) { _contentLabel.Text = "No market data."; return; }

        _itemIndex = System.Math.Clamp(_itemIndex, 0, items.Count - 1);
        var item = items[_itemIndex];
        _selectedItemId = item.ItemId;

        var text = "=== PRICE ANALYTICS ===\n";
        text += $"Item: {item.Name} ({_itemIndex + 1}/{items.Count})\n";
        text += "Left/Right: Change item\n\n";

        // Current stats
        text += $"-- Current --\n";
        text += $"Price: {item.LastPrice:F1} ISK | Base: {item.BasePrice:F1}\n";
        text += $"Volume: {item.VolumeTraded} | Supply: {item.SupplyFactor:F2} | Demand: {item.DemandFactor:F2}\n";

        var priceEngine = PriceCalculationEngine.Instance;
        if (priceEngine != null)
        {
            var stats = priceEngine.GetPriceStats(item.ItemId);
            text += $"\n-- Analytics --\n";
            text += $"Avg: {stats.AveragePrice:F1} | High: {stats.HighPrice:F1} | Low: {stats.LowPrice:F1}\n";
            text += $"Volatility: {stats.Volatility:F3}\n";

            double changeFromBase = item.BasePrice > 0 ? (item.LastPrice - item.BasePrice) / item.BasePrice * 100 : 0;
            text += $"Change from base: {changeFromBase:+0.0;-0.0}%\n";

            string trend = item.LastPrice > stats.AveragePrice ? "BULLISH" : item.LastPrice < stats.AveragePrice ? "BEARISH" : "NEUTRAL";
            text += $"Trend: {trend}\n";
        }

        // Order book
        var engine = OrderMatchingEngine.Instance;
        if (engine != null)
        {
            var book = engine.GetOrderBook(item.ItemId);
            if (book != null)
            {
                text += $"\n-- Order Book --\n";
                text += $"Bid: {book.BestBid:F1} | Ask: {book.BestAsk:F1}\n";
                text += $"Spread: {book.Spread:F1}\n";
                text += $"Buy depth: {book.BuyDepth} | Sell depth: {book.SellDepth}\n";
            }
        }

        // Price history chart
        var history = service.GetPriceHistory(item.ItemId);
        if (history.Count > 0)
        {
            text += $"\n-- History ({history.Count} points) --\n";
            double min = history.Min(h => h.LowPrice);
            double max = history.Max(h => h.HighPrice);
            double range = max - min;
            if (range < 0.01) range = 1;

            foreach (var h in history.Take(10))
            {
                int barLen = (int)((h.AvgPrice - min) / range * 20);
                text += $"{h.Date:MM/dd} {"".PadLeft(System.Math.Max(1, barLen), '#'),-20} {h.AvgPrice:F1}\n";
            }
        }

        // Active market events
        var events = service.GetActiveEvents();
        if (events.Count > 0)
        {
            text += $"\n-- Active Events --\n";
            foreach (var evt in events.Take(3))
                text += $"  {evt.Description} (x{evt.PriceModifier:F2})\n";
        }

        text += "\nH: Close";
        _contentLabel.Text = text;
    }
}
