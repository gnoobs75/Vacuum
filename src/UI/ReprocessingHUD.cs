using Godot;
using System.Linq;
using Vacuum.Data;
using Vacuum.Systems.Mining;

namespace Vacuum.UI;

/// <summary>
/// WO-166: Reprocessing station HUD. Shows when docked at a station.
/// Displays ore inventory, reprocessing controls, and mineral wallet.
/// </summary>
public partial class ReprocessingHUD : PanelContainer
{
    [Export] public NodePath ReprocessorPath { get; set; } = "";
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private OreReprocessor? _reprocessor;
    private CargoHold? _cargoHold;
    private Label? _infoLabel;
    private double _updateTimer;

    public override void _Ready()
    {
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.05f, 0.02f, 0.08f, 0.85f);
        style.SetCornerRadiusAll(4);
        style.BorderColor = new Color(0.5f, 0.3f, 0.7f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        AddChild(margin);

        _infoLabel = new Label();
        _infoLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.7f, 1f));
        _infoLabel.AddThemeFontSizeOverride("font_size", 13);
        margin.AddChild(_infoLabel);

        CallDeferred(MethodName.ConnectNodes);
    }

    private void ConnectNodes()
    {
        if (!string.IsNullOrEmpty(ReprocessorPath))
            _reprocessor = GetNodeOrNull<OreReprocessor>(ReprocessorPath);
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    public override void _Process(double delta)
    {
        bool docked = _reprocessor?.IsDocked ?? false;
        Visible = docked;
        if (!docked || _infoLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.2) return;
        _updateTimer = 0;

        var text = "--- STATION SERVICES ---\n";
        text += "Press R to reprocess ore\n";
        text += "Press F to undock\n\n";

        // Ore in cargo
        text += "--- ORE INVENTORY ---\n";
        if (_cargoHold != null)
        {
            var oreItems = _cargoHold.Items.Values
                .Where(i => OreDatabase.Ores.ContainsKey(i.ItemId))
                .ToList();

            if (oreItems.Count > 0)
            {
                foreach (var item in oreItems)
                {
                    var def = OreDatabase.Ores[item.ItemId];
                    int batches = item.Quantity / def.ReprocessBatchSize;
                    text += $"  {item.Name}: {item.Quantity} ({batches} batches)\n";
                }
            }
            else
            {
                text += "  No ore in cargo\n";
            }
        }

        text += "\n--- MINERAL WALLET ---\n";
        if (_reprocessor != null && _reprocessor.MineralWallet.Any())
        {
            float totalValue = 0;
            foreach (var (mineralId, qty) in _reprocessor.MineralWallet.OrderByDescending(m => m.Value))
            {
                string name = mineralId;
                float value = 0;
                if (OreDatabase.Minerals.TryGetValue(mineralId, out var mDef))
                {
                    name = mDef.Name;
                    value = qty * mDef.BaseValue;
                }
                totalValue += value;
                text += $"  {name}: {qty} ({value:N0} cr)\n";
            }
            text += $"\n  Total value: {totalValue:N0} credits\n";
        }
        else
        {
            text += "  (empty)\n";
        }

        _infoLabel.Text = text;
    }
}
