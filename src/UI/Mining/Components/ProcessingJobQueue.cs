using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining.Components;

/// <summary>
/// Displays active and queued reprocessing operations with progress tracking.
/// </summary>
public partial class ProcessingJobQueue : VBoxContainer
{
    private Label? _queueLabel;
    private double _updateTimer;

    public override void _Ready()
    {
        var title = new Label { Text = "PROCESSING QUEUE" };
        title.AddThemeColorOverride("font_color", new Color(0.8f, 0.5f, 1f));
        title.AddThemeFontSizeOverride("font_size", 12);
        AddChild(title);

        _queueLabel = new Label();
        _queueLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.9f));
        _queueLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_queueLabel);
    }

    public override void _Process(double delta)
    {
        _updateTimer += delta;
        if (_updateTimer < 0.3) return;
        _updateTimer = 0;

        var queue = ReprocessingQueue.Instance;
        if (queue == null || _queueLabel == null) return;

        string text = "";

        var current = queue.CurrentJob;
        if (current != null)
            text += $"[Active] {current.OreId} x{current.Quantity} ({current.Progress * 100:F0}%)\n";

        foreach (var job in queue.PendingJobs)
            text += $"[Queued] {job.OreId} x{job.Quantity}\n";

        _queueLabel.Text = text.Length > 0 ? text : "(no jobs)";
    }
}
