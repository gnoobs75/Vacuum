using Godot;

namespace Vacuum.Systems.Flight;

/// <summary>
/// WO-34 + WO-56 + WO-40: Connects input to ship physics.
/// Attach as a child of the ShipPhysics node.
/// </summary>
public partial class ShipFlightController : Node
{
    [Export] public float MouseSensitivity { get; set; } = 0.003f;
    [Export] public float GamepadSensitivity { get; set; } = 3f;
    [Export] public NodePath ShipPhysicsPath { get; set; } = "..";

    private ShipPhysics? _ship;
    private Vector2 _mouseMotion;
    private bool _mouseCapture = true;

    public override void _Ready()
    {
        _ship = GetNode<ShipPhysics>(ShipPhysicsPath);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && _mouseCapture)
        {
            _mouseMotion = mouseMotion.Relative * MouseSensitivity;
        }

        if (@event.IsActionPressed("ui_cancel"))
        {
            _mouseCapture = !_mouseCapture;
            Input.MouseMode = _mouseCapture
                ? Input.MouseModeEnum.Captured
                : Input.MouseModeEnum.Visible;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ship == null) return;

        // Thrust: WASD + Space/Ctrl
        var thrust = Vector3.Zero;
        if (Input.IsActionPressed("flight_forward")) thrust.Z -= 1f;
        if (Input.IsActionPressed("flight_backward")) thrust.Z += 1f;
        if (Input.IsActionPressed("flight_strafe_left")) thrust.X -= 1f;
        if (Input.IsActionPressed("flight_strafe_right")) thrust.X += 1f;
        if (Input.IsActionPressed("flight_ascend")) thrust.Y += 1f;
        if (Input.IsActionPressed("flight_descend")) thrust.Y -= 1f;

        _ship.SetThrustInput(thrust);
        _ship.SetRotationInput(_mouseMotion);
        _ship.SetBoosting(Input.IsActionPressed("flight_boost"));
        _ship.SetBraking(Input.IsActionPressed("flight_brake"));

        // Reset mouse motion each frame
        _mouseMotion = Vector2.Zero;
    }
}
