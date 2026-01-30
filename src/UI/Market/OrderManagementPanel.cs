using Godot;
using System.Linq;
using Vacuum.Data.Enums;
using Vacuum.Services.Market;

namespace Vacuum.UI.Market;

/// <summary>
/// WO-99: Buy/sell order management interface showing active orders, history, and portfolio.
/// </summary>
public partial class OrderManagementPanel : PanelContainer
{
    private Label? _contentLabel;
    private double _updateTimer;
    private bool _visible;
    private string _playerId = "player";

    public override void _Ready()
    {
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.04f, 0.02f, 0.08f, 0.92f);
        style.SetCornerRadiusAll(6);
        style.BorderColor = new Color(0.6f, 0.3f, 0.7f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(340, 380) };
        margin.AddChild(scroll);

        _contentLabel = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
        _contentLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.75f, 1f));
        _contentLabel.AddThemeFontSizeOverride("font_size", 12);
        scroll.AddChild(_contentLabel);
    }

    public void Toggle()
    {
        _visible = !_visible;
        Visible = _visible;
    }

    public override void _Process(double delta)
    {
        if (!_visible || _contentLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.5) return;
        _updateTimer = 0;

        var service = MarketService.Instance;
        if (service == null) { _contentLabel.Text = "Market unavailable."; return; }

        var orders = service.GetPlayerOrders(_playerId);
        var active = orders.Where(o => o.Status == MarketOrderStatus.Active || o.Status == MarketOrderStatus.Partial).ToList();
        var completed = orders.Where(o => o.Status == MarketOrderStatus.Filled).ToList();

        var text = "=== MY ORDERS ===\n\n";

        // Active orders
        text += $"-- Active ({active.Count}) --\n";
        foreach (var o in active.Take(10))
        {
            string type = o.IsBuyOrder ? "BUY" : "SELL";
            string status = o.Status == MarketOrderStatus.Partial ? $"({o.FilledQuantity}/{o.Quantity})" : "";
            text += $"  {type} {o.ItemTypeId}: {o.Quantity}x @ {o.Price:F1} {status}\n";
        }

        if (active.Count == 0)
            text += "  (no active orders)\n";

        // Completed
        text += $"\n-- Filled ({completed.Count}) --\n";
        foreach (var o in completed.Take(5))
        {
            string type = o.IsBuyOrder ? "BUY" : "SELL";
            text += $"  {type} {o.ItemTypeId}: {o.Quantity}x @ {o.Price:F1}\n";
        }

        // Portfolio summary
        var totalBuyValue = active.Where(o => o.IsBuyOrder).Sum(o => o.RemainingQuantity * o.Price);
        var totalSellValue = active.Where(o => !o.IsBuyOrder).Sum(o => o.RemainingQuantity * o.Price);
        text += $"\n-- Portfolio --\n";
        text += $"Open buy value: {totalBuyValue:F0} ISK\n";
        text += $"Open sell value: {totalSellValue:F0} ISK\n";
        text += $"Total trades: {completed.Count}\n";

        _contentLabel.Text = text;
    }
}
