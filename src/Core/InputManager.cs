using System.Collections.Generic;
using Godot;

namespace Vacuum.Core;

/// <summary>
/// WO-56: Central input system managing control schemes and action routing.
/// Autoload singleton that provides input state to all game systems.
/// </summary>
public partial class InputManager : Node
{
    public static InputManager? Instance { get; private set; }

    public enum ControlScheme { KeyboardMouse, Gamepad }
    public ControlScheme ActiveScheme { get; private set; } = ControlScheme.KeyboardMouse;

    [Signal] public delegate void ControlSchemeChangedEventHandler(int scheme);

    // Input action categories for routing
    public static class Actions
    {
        // Flight
        public const string Forward = "flight_forward";
        public const string Backward = "flight_backward";
        public const string StrafeLeft = "flight_strafe_left";
        public const string StrafeRight = "flight_strafe_right";
        public const string Ascend = "flight_ascend";
        public const string Descend = "flight_descend";
        public const string Boost = "flight_boost";
        public const string Brake = "flight_brake";

        // Navigation
        public const string WarpDrive = "warp_drive";
        public const string ToggleAutopilot = "toggle_autopilot";
        public const string AddBookmark = "add_bookmark";
        public const string ToggleMap = "toggle_map";

        // Targeting
        public const string TargetLock = "target_lock";
    }

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Input(InputEvent @event)
    {
        // Auto-detect control scheme switch
        if (@event is InputEventJoypadButton or InputEventJoypadMotion)
        {
            if (ActiveScheme != ControlScheme.Gamepad)
            {
                ActiveScheme = ControlScheme.Gamepad;
                EmitSignal(SignalName.ControlSchemeChanged, (int)ActiveScheme);
            }
        }
        else if (@event is InputEventKey or InputEventMouseButton or InputEventMouseMotion)
        {
            if (ActiveScheme != ControlScheme.KeyboardMouse)
            {
                ActiveScheme = ControlScheme.KeyboardMouse;
                EmitSignal(SignalName.ControlSchemeChanged, (int)ActiveScheme);
            }
        }
    }
}
