using Godot;
using System.Linq;
using Vacuum.Data;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining;

/// <summary>
/// WO-81: Ore processing interface for reprocessing ore into minerals with
/// batch processing, efficiency display, and queue management.
/// </summary>
public partial class OreProcessingPanel : PanelContainer
{
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private CargoHold? _cargoHold;
    private Label? _contentLabel;
    private double _updateTimer;
    private bool _visible;

    public override void _Ready()
    {
        Visible = false;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.05f, 0.02f, 0.1f, 0.9f);
        style.SetCornerRadiusAll(6);
        style.BorderColor = new Color(0.5f, 0.2f, 0.6f, 0.6f);
        style.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", style);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        AddChild(margin);

        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(260, 300) };
        margin.AddChild(scroll);

        _contentLabel = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
        _contentLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.7f, 1f));
        _contentLabel.AddThemeFontSizeOverride("font_size", 12);
        scroll.AddChild(_contentLabel);

        CallDeferred(MethodName.ConnectNodes);
    }

    private void ConnectNodes()
    {
        if (!string.IsNullOrEmpty(CargoHoldPath))
            _cargoHold = GetNodeOrNull<CargoHold>(CargoHoldPath);
    }

    public override void _Process(double delta)
    {
        if (!_visible || _contentLabel == null) return;

        _updateTimer += delta;
        if (_updateTimer < 0.3) return;
        _updateTimer = 0;

        var text = "=== ORE PROCESSING ===\n\n";

        // Ore inventory
        text += "-- Ore Inventory --\n";
        if (_cargoHold != null && _cargoHold.Items.Any())
        {
            foreach (var item in _cargoHold.Items.Values.Where(i => OreDatabase.Ores.ContainsKey(i.ItemId)))
            {
                var oreDef = OreDatabase.Ores[item.ItemId];
                float sellValue = item.Quantity * oreDef.BaseValue;
                text += $"  {item.Name}: {item.Quantity} (sell: {sellValue:F0} ISK)\n";

                // Preview reprocessing
                var preview = Services.Mining.YieldCalculator.EstimateReprocessingOutput(
                    item.ItemId, item.Quantity, 0.7f);
                if (preview.Count > 0)
                {
                    text += "    -> ";
                    text += string.Join(", ", preview.Select(m =>
                    {
                        string mName = OreDatabase.Minerals.TryGetValue(m.Key, out var md) ? md.Name : m.Key;
                        return $"{mName}: {m.Value}";
                    }));
                    text += "\n";
                }
            }
        }
        else
        {
            text += "  (no ore in cargo)\n";
        }

        // Reprocessing queue
        var queue = ReprocessingQueue.Instance;
        if (queue != null)
        {
            text += $"\n-- Queue ({queue.QueueLength} pending) --\n";
            var current = queue.CurrentJob;
            if (current != null)
                text += $"  Processing: {current.OreId} ({current.Progress * 100:F0}%)\n";
        }

        // Mineral inventory
        var minerals = MineralInventoryManager.Instance;
        if (minerals != null && minerals.TotalMineralCount > 0)
        {
            text += $"\n-- Minerals (value: {minerals.GetTotalValue():F0} ISK) --\n";
            foreach (var (id, qty) in minerals.Minerals)
            {
                string name = OreDatabase.Minerals.TryGetValue(id, out var md) ? md.Name : id;
                text += $"  {name}: {qty}\n";
            }
        }

        text += "\nDock (F) + Reprocess (R)";

        _contentLabel.Text = text;
    }

    public new void Show() { _visible = true; Visible = true; }
    public new void Hide() { _visible = false; Visible = false; }
    public void Toggle() { if (_visible) Hide(); else Show(); }
}
