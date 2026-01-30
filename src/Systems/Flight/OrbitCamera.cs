using Godot;

namespace Vacuum.Systems.Flight;

/// <summary>
/// Third-person orbit camera that follows a target node.
/// Mouse wheel zooms in/out. Camera inherits rotation from the ship.
/// </summary>
public partial class OrbitCamera : Camera3D
{
    [Export] public NodePath TargetPath { get; set; } = "";
    [Export] public float MinDistance { get; set; } = 5f;
    [Export] public float MaxDistance { get; set; } = 100f;
    [Export] public float DefaultDistance { get; set; } = 25f;
    [Export] public float ZoomSpeed { get; set; } = 2f;
    [Export] public float ZoomSmoothSpeed { get; set; } = 8f;
    [Export] public float HeightOffset { get; set; } = 5f;
    [Export] public float PitchAngle { get; set; } = 15f;

    private Node3D? _target;
    private float _currentDistance;
    private float _targetDistance;

    public override void _Ready()
    {
        _targetDistance = DefaultDistance;
        _currentDistance = DefaultDistance;

        if (!string.IsNullOrEmpty(TargetPath))
            _target = GetNodeOrNull<Node3D>(TargetPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                _targetDistance = Mathf.Max(MinDistance, _targetDistance - ZoomSpeed);
                GetViewport().SetInputAsHandled();
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                _targetDistance = Mathf.Min(MaxDistance, _targetDistance + ZoomSpeed);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_target == null) return;

        float dt = (float)delta;
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, ZoomSmoothSpeed * dt);

        // Position behind and above the ship
        var shipBasis = _target.GlobalTransform.Basis;
        Vector3 back = shipBasis.Z.Normalized();
        Vector3 up = shipBasis.Y.Normalized();

        float pitchRad = Mathf.DegToRad(PitchAngle);
        Vector3 offset = (back * Mathf.Cos(pitchRad) + up * Mathf.Sin(pitchRad)) * _currentDistance;
        offset += up * HeightOffset;

        GlobalPosition = _target.GlobalPosition + offset;
        LookAt(_target.GlobalPosition, up);
    }
}
