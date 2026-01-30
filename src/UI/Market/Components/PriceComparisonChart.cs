using Godot;
using System.Collections.Generic;
using System.Linq;
using Vacuum.Data.Models;

namespace Vacuum.UI.Market.Components;

/// <summary>
/// Text-based price comparison display across time periods.
/// </summary>
public partial class PriceComparisonChart : VBoxContainer
{
    private Label? _chartLabel;

    public override void _Ready()
    {
        var title = new Label { Text = "PRICE TREND" };
        title.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 0.5f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _chartLabel = new Label();
        _chartLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.7f));
        _chartLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_chartLabel);
    }

    public void UpdateChart(List<PriceHistoryData> history, string itemName)
    {
        if (_chartLabel == null) return;
        if (history.Count == 0) { _chartLabel.Text = "No price data."; return; }

        double min = history.Min(h => h.LowPrice);
        double max = history.Max(h => h.HighPrice);
        double avg = history.Average(h => h.AvgPrice);
        double latest = history[0].AvgPrice;
        double change = history.Count > 1 ? latest - history[^1].AvgPrice : 0;
        string changeStr = change >= 0 ? $"+{change:F1}" : $"{change:F1}";

        string text = $"{itemName}\n";
        text += $"Latest: {latest:F1} ({changeStr})\n";
        text += $"High: {max:F1} | Low: {min:F1}\n";
        text += $"Avg: {avg:F1} | Points: {history.Count}\n\n";

        // Simple ASCII bar chart (last 10 entries)
        double range = max - min;
        if (range < 0.01) range = 1;

        foreach (var h in history.Take(10))
        {
            int barLen = (int)((h.AvgPrice - min) / range * 20);
            text += $"{h.Date:MM/dd} {"".PadLeft(System.Math.Max(1, barLen), '#'),-20} {h.AvgPrice:F1}\n";
        }

        _chartLabel.Text = text;
    }
}
