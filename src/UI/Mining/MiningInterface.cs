using Godot;
using Vacuum.Systems.Mining;

namespace Vacuum.UI.Mining;

/// <summary>
/// WO-159: Comprehensive mining interface integrating laser controls, cargo display,
/// statistics, and alerts in a unified layout.
/// </summary>
public partial class MiningInterface : Control
{
    [Export] public NodePath MiningLaserPath { get; set; } = "";
    [Export] public NodePath TargetingPath { get; set; } = "";
    [Export] public NodePath CargoHoldPath { get; set; } = "";

    private MiningLaserControlsPanel? _laserPanel;
    private Components.CargoStatusDisplay? _cargoDisplay;
    private Components.MiningControlPanel? _controlPanel;
    private Components.MiningAlertsSystem? _alerts;

    public override void _Ready()
    {
        // Build integrated mining interface
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 8);
        AddChild(hbox);

        // Left: laser controls
        _laserPanel = new MiningLaserControlsPanel
        {
            MiningLaserPath = MiningLaserPath,
            TargetingPath = TargetingPath
        };
        hbox.AddChild(_laserPanel);

        // Center: control panel
        _controlPanel = new Components.MiningControlPanel();
        hbox.AddChild(_controlPanel);

        // Right: cargo status
        _cargoDisplay = new Components.CargoStatusDisplay
        {
            CargoHoldPath = CargoHoldPath
        };
        hbox.AddChild(_cargoDisplay);

        // Alerts overlay
        _alerts = new Components.MiningAlertsSystem();
        AddChild(_alerts);
    }
}
