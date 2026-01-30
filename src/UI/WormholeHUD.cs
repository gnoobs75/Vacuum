using Godot;
using Vacuum.Core;
using Vacuum.Data.Enums;
using Vacuum.Data.Models;
using Vacuum.Systems.Navigation;

namespace Vacuum.UI;

/// <summary>
/// WO-100: Wormhole navigation interface showing stability, mass limits,
/// collapse timers, and transit controls.
/// </summary>
public partial class WormholeHUD : Control
{
    private Label? _stabilityLabel;
    private ProgressBar? _stabilityBar;
    private Label? _massLabel;
    private ProgressBar? _massBar;
    private Label? _collapseTimerLabel;
    private Label? _destinationLabel;
    private Label? _classificationLabel;
    private Button? _transitButton;

    private string? _selectedWormholeId;

    public override void _Ready()
    {
        _stabilityLabel = GetNodeOrNull<Label>("%StabilityLabel");
        _stabilityBar = GetNodeOrNull<ProgressBar>("%StabilityBar");
        _massLabel = GetNodeOrNull<Label>("%MassLabel");
        _massBar = GetNodeOrNull<ProgressBar>("%MassBar");
        _collapseTimerLabel = GetNodeOrNull<Label>("%CollapseTimerLabel");
        _destinationLabel = GetNodeOrNull<Label>("%DestinationLabel");
        _classificationLabel = GetNodeOrNull<Label>("%ClassificationLabel");
        _transitButton = GetNodeOrNull<Button>("%TransitButton");

        _transitButton?.Connect("pressed", Callable.From(OnTransitPressed));
        Visible = false;

        GameEventBus.Instance?.Connect(GameEventBus.SignalName.WormholeDiscovered,
            Callable.From<string>(OnWormholeDiscovered));
    }

    public void ShowWormhole(string wormholeId)
    {
        _selectedWormholeId = wormholeId;
        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
        _selectedWormholeId = null;
    }

    public override void _Process(double delta)
    {
        if (!Visible || _selectedWormholeId == null) return;

        var wh = WormholeSystem.Instance?.GetWormhole(_selectedWormholeId);
        if (wh == null) { Hide(); return; }

        if (_stabilityLabel != null)
            _stabilityLabel.Text = $"Stability: {wh.Stability * 100:F1}%";
        if (_stabilityBar != null)
            _stabilityBar.Value = wh.Stability * 100;

        float massFraction = wh.MassLimit > 0 ? wh.MassUsed / wh.MassLimit : 0;
        if (_massLabel != null)
            _massLabel.Text = $"Mass: {wh.MassUsed:F0} / {wh.MassLimit:F0}";
        if (_massBar != null)
            _massBar.Value = massFraction * 100;

        var timeLeft = wh.CollapseTime - System.DateTime.UtcNow;
        if (_collapseTimerLabel != null)
            _collapseTimerLabel.Text = timeLeft.TotalSeconds > 0
                ? $"Collapse: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}"
                : "COLLAPSING";

        if (_destinationLabel != null)
            _destinationLabel.Text = $"Dest: {wh.DestinationSystem}";

        if (_classificationLabel != null)
        {
            _classificationLabel.Text = wh.Classification.ToString().ToUpper();
            _classificationLabel.Modulate = wh.Classification switch
            {
                WormholeClassification.Stable => Colors.Green,
                WormholeClassification.Unstable => Colors.Yellow,
                WormholeClassification.Critical => Colors.Orange,
                WormholeClassification.Collapsing => Colors.Red,
                _ => Colors.White
            };
        }

        if (_transitButton != null)
            _transitButton.Disabled = wh.Classification == WormholeClassification.Collapsing;
    }

    private void OnTransitPressed()
    {
        if (_selectedWormholeId == null) return;
        // Ship mass would come from the player's ship data
        WormholeSystem.Instance?.TransitWormhole(_selectedWormholeId, "player_ship", 10f);
    }

    private void OnWormholeDiscovered(string wormholeId)
    {
        GD.Print($"[WormholeHUD] New wormhole discovered: {wormholeId}");
    }
}
