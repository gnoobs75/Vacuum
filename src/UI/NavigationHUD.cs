using Godot;
using Vacuum.Core;
using Vacuum.Data.Enums;
using Vacuum.Systems.Flight;
using Vacuum.Systems.Navigation;

namespace Vacuum.UI;

/// <summary>
/// WO-55: HUD displaying speed, heading, warp status, autopilot, target info.
/// Attach to a Control node in the UI scene.
/// </summary>
public partial class NavigationHUD : Control
{
    [Export] public NodePath ShipPath { get; set; } = "";
    [Export] public NodePath WarpDrivePath { get; set; } = "";
    [Export] public NodePath AutopilotPath { get; set; } = "";

    // UI element references (assign in editor or find by name)
    private Label? _speedLabel;
    private Label? _headingLabel;
    private Label? _warpStatusLabel;
    private Label? _autopilotLabel;
    private Label? _positionLabel;
    private Label? _targetLabel;
    private ProgressBar? _warpProgress;

    private ShipPhysics? _ship;
    private WarpDriveSystem? _warpDrive;
    private AutopilotSystem? _autopilot;

    public override void _Ready()
    {
        // Find UI elements by name
        _speedLabel = GetNodeOrNull<Label>("%SpeedLabel");
        _headingLabel = GetNodeOrNull<Label>("%HeadingLabel");
        _warpStatusLabel = GetNodeOrNull<Label>("%WarpStatusLabel");
        _autopilotLabel = GetNodeOrNull<Label>("%AutopilotLabel");
        _positionLabel = GetNodeOrNull<Label>("%PositionLabel");
        _targetLabel = GetNodeOrNull<Label>("%TargetLabel");
        _warpProgress = GetNodeOrNull<ProgressBar>("%WarpProgress");

        // Defer ship lookup until tree is ready
        CallDeferred(MethodName.ConnectShipNodes);
    }

    private void ConnectShipNodes()
    {
        if (!string.IsNullOrEmpty(ShipPath))
            _ship = GetNodeOrNull<ShipPhysics>(ShipPath);
        if (!string.IsNullOrEmpty(WarpDrivePath))
            _warpDrive = GetNodeOrNull<WarpDriveSystem>(WarpDrivePath);
        if (!string.IsNullOrEmpty(AutopilotPath))
            _autopilot = GetNodeOrNull<AutopilotSystem>(AutopilotPath);
    }

    public override void _Process(double delta)
    {
        if (_ship == null) return;

        // Speed
        float speed = _ship.CurrentSpeed;
        if (_speedLabel != null)
            _speedLabel.Text = $"SPD: {speed:F0} m/s";

        // Heading (forward direction)
        if (_headingLabel != null)
        {
            var forward = -_ship.GlobalTransform.Basis.Z;
            _headingLabel.Text = $"HDG: ({forward.X:F1}, {forward.Y:F1}, {forward.Z:F1})";
        }

        // Position
        if (_positionLabel != null)
        {
            var pos = _ship.GlobalPosition;
            _positionLabel.Text = $"POS: ({pos.X:F0}, {pos.Y:F0}, {pos.Z:F0})";
        }

        // Warp status
        if (_warpDrive != null && _warpStatusLabel != null)
        {
            _warpStatusLabel.Text = _warpDrive.State switch
            {
                WarpState.Idle => "WARP: Ready",
                WarpState.Aligning => "WARP: Aligning...",
                WarpState.Accelerating => "WARP: Accelerating...",
                WarpState.InWarp => $"WARP: In Warp ({_warpDrive.WarpProgress * 100:F0}%)",
                WarpState.Decelerating => "WARP: Decelerating...",
                _ => "WARP: --"
            };
        }

        if (_warpDrive != null && _warpProgress != null)
        {
            _warpProgress.Value = _warpDrive.WarpProgress * 100;
            _warpProgress.Visible = _warpDrive.State != WarpState.Idle;
        }

        // Autopilot
        if (_autopilot != null && _autopilotLabel != null)
        {
            _autopilotLabel.Text = _autopilot.Enabled
                ? $"AP: ON (WP {_autopilot.CurrentWaypointIndex + 1})"
                : "AP: OFF";
        }
    }
}
