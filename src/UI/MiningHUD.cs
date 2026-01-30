using Godot;
using System.Linq;
using Vacuum.Data;
using Vacuum.Systems.Mining;

namespace Vacuum.UI;

/// <summary>
/// WO-159: Mining control and status HUD. Shows target info, mining cycle,
/// cargo hold contents, and mining statistics. Visible when mining is active
/// or target is locked.
/// </summary>
public partial class MiningHUD : PanelContainer
{
    [Export] public NodePath TargetingPath { get; set; } = "";
    [Export] public NodePath MiningLaserPath { get; set; } = "";
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private TargetingSystem? _targeting;
    private MiningLaserSystem? _miningLaser;
    private CargoHold? _cargoHold;
    private Label? _infoLabel;
    private double _updateTimer;

    public override void _Ready()
    {
        Visible = false;

        // Style
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0f, 0.05f, 0.1f, 0.8f);
        style.SetCornerRadiusAll(4);
        style.BorderColor = new Color(0.1f, 0.4f, 0.6f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        AddChild(margin);

        _infoLabel = new Label();
        _infoLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 1f));
        _infoLabel.AddThemeFontSizeOverride("font_size", 13);
        margin.AddChild(_infoLabel);

        CallDeferred(MethodName.ConnectNodes);
    }

    private void ConnectNodes()
    {
        if (!string.IsNullOrEmpty(TargetingPath))
            _targeting = GetNodeOrNull<TargetingSystem>(TargetingPath);
        if (!string.IsNullOrEmpty(MiningLaserPath))
            _miningLaser = GetNodeOrNull<MiningLaserSystem>(MiningLaserPath);
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    public override void _Process(double delta)
    {
        bool shouldShow = (_targeting?.HasTarget ?? false) || (_miningLaser?.IsActive ?? false);
        Visible = shouldShow;
        if (!shouldShow || _infoLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.15) return;
        _updateTimer = 0;

        var text = "--- MINING ---\n";

        // Target info
        if (_targeting != null && _targeting.HasTarget)
        {
            var target = _targeting.LockedTarget!;
            var mineable = target.GetNodeOrNull<MineableAsteroid>("MineableAsteroid");
            float dist = _targeting.GetTargetDistance();
            text += $"Target: {target.Name} ({dist:F0}m)\n";

            if (mineable != null)
            {
                string oreName = mineable.State.OreId;
                if (OreDatabase.Ores.TryGetValue(mineable.State.OreId, out var oreDef))
                    oreName = oreDef.Name;

                float pct = mineable.State.TotalOre > 0
                    ? mineable.State.RemainingOre / mineable.State.TotalOre * 100f
                    : 0f;

                text += $"Ore: {oreName}\n";
                text += $"Remaining: {mineable.State.RemainingOre:F0} / {mineable.State.TotalOre:F0} ({pct:F0}%)\n";
            }
        }
        else
        {
            text += "No target (T to lock)\n";
        }

        text += "\n";

        // Mining laser status
        if (_miningLaser != null)
        {
            if (_miningLaser.IsActive)
            {
                string oreName = _miningLaser.CurrentOreId ?? "?";
                if (_miningLaser.CurrentOreId != null && OreDatabase.Ores.TryGetValue(_miningLaser.CurrentOreId, out var od))
                    oreName = od.Name;

                text += $"MINING: {oreName}\n";
                text += $"Cycle: {_miningLaser.CyclePercent:F0}%";
                // Simple progress bar
                int bars = (int)(_miningLaser.CyclePercent / 5);
                text += $" [{"".PadLeft(bars, '#')}{"".PadLeft(20 - bars, '-')}]\n";
                text += $"Lasers: {_miningLaser.ActiveLaserCount}\n";
            }
            else
            {
                text += "Laser: IDLE (F1 to mine)\n";
            }

            text += $"\nYield/min: {_miningLaser.YieldPerMinute:F0}\n";
            text += $"Cycles: {_miningLaser.TotalCycles} | Mined: {_miningLaser.TotalOreMined}\n";
        }

        text += "\n";

        // Cargo hold
        if (_cargoHold != null)
        {
            text += $"--- CARGO ({_cargoHold.UsedVolume:F1}/{_cargoHold.MaxVolume:F0} m³) ---\n";
            foreach (var item in _cargoHold.Items.Values.OrderByDescending(i => i.Quantity))
            {
                text += $"  {item.Name}: {item.Quantity} ({item.TotalVolume:F1} m³)\n";
            }
            if (!_cargoHold.Items.Any())
                text += "  (empty)\n";
        }

        _infoLabel.Text = text;
    }
}
