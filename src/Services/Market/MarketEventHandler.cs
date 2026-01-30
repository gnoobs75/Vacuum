using Godot;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Services.Market;

/// <summary>
/// Handles market event emission via Godot signals for decoupled UI updates.
/// </summary>
public partial class MarketEventHandler : Node
{
    public static MarketEventHandler? Instance { get; private set; }

    [Signal] public delegate void ItemPriceChangedEventHandler(string itemId, double oldPrice, double newPrice);
    [Signal] public delegate void OrderStatusChangedEventHandler(string orderId, int newStatus);
    [Signal] public delegate void MarketEventStartedEventHandler(string eventId, string description);
    [Signal] public delegate void MarketEventEndedEventHandler(string eventId);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[MarketEventHandler] Ready.");
    }

    public void NotifyPriceChange(string itemId, double oldPrice, double newPrice)
    {
        EmitSignal(SignalName.ItemPriceChanged, itemId, oldPrice, newPrice);
    }

    public void NotifyOrderStatus(string orderId, MarketOrderStatus status)
    {
        EmitSignal(SignalName.OrderStatusChanged, orderId, (int)status);
    }

    public void NotifyMarketEvent(MarketEventData evt)
    {
        EmitSignal(SignalName.MarketEventStarted, evt.EventId, evt.Description);
    }
}
