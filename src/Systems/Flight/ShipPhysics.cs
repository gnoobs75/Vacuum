using Godot;

namespace Vacuum.Systems.Flight;

/// <summary>
/// WO-34: Core ship physics with thrust vectoring, inertia, and Newtonian movement.
/// Attach to a RigidBody3D node representing the player ship or NPC ship.
/// </summary>
public partial class ShipPhysics : RigidBody3D
{
    [Export] public float ThrustForce { get; set; } = 50f;
    [Export] public float StrafeForce { get; set; } = 30f;
    [Export] public float VerticalForce { get; set; } = 30f;
    [Export] public float BoostMultiplier { get; set; } = 3f;
    [Export] public float MaxSpeed { get; set; } = 200f;
    [Export] public float RotationSpeed { get; set; } = 2f;
    [Export] public float RollSpeed { get; set; } = 1.5f;
    [Export] public float LinearDamping { get; set; } = 0.3f;
    [Export] public float BrakeForce { get; set; } = 80f;
    [Export] public float Mass_ { get; set; } = 10f;
    [Export] public float SignatureRadius { get; set; } = 50f;

    private Vector3 _thrustInput;
    private Vector2 _rotationInput;
    private float _rollInput;
    private bool _boosting;
    private bool _braking;

    public Vector3 CurrentVelocity => LinearVelocity;
    public float CurrentSpeed => LinearVelocity.Length();
    public float SpeedFraction => MaxSpeed > 0 ? CurrentSpeed / MaxSpeed : 0f;

    public override void _Ready()
    {
        GravityScale = 0;
        base.Mass = Mass_;
        base.LinearDamp = LinearDamping;
    }

    public void SetThrustInput(Vector3 input)
    {
        _thrustInput = input.Normalized();
    }

    public void SetRotationInput(Vector2 input)
    {
        _rotationInput = input;
    }

    public void SetRollInput(float input)
    {
        _rollInput = input;
    }

    public void SetBoosting(bool boosting)
    {
        _boosting = boosting;
    }

    public void SetBraking(bool braking)
    {
        _braking = braking;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        ApplyRotation(dt);
        ApplyThrust(dt);
        ClampSpeed();
    }

    private void ApplyRotation(float dt)
    {
        // Pitch and yaw from mouse/stick
        float pitch = -_rotationInput.Y * RotationSpeed * dt;
        float yaw = -_rotationInput.X * RotationSpeed * dt;
        float roll = _rollInput * RollSpeed * dt;

        RotateObjectLocal(Vector3.Right, pitch);
        RotateObjectLocal(Vector3.Up, yaw);
        RotateObjectLocal(Vector3.Forward, roll);
    }

    private void ApplyThrust(float dt)
    {
        if (_braking)
        {
            // Apply force opposing current velocity
            if (LinearVelocity.LengthSquared() > 0.01f)
            {
                Vector3 brakeDir = -LinearVelocity.Normalized();
                ApplyCentralForce(brakeDir * BrakeForce);
            }
            return;
        }

        if (_thrustInput.LengthSquared() < 0.001f)
            return;

        float multiplier = _boosting ? BoostMultiplier : 1f;

        // Transform thrust input from local to global space
        Vector3 globalThrust = GlobalTransform.Basis * new Vector3(
            _thrustInput.X * StrafeForce,
            _thrustInput.Y * VerticalForce,
            _thrustInput.Z * ThrustForce
        );

        ApplyCentralForce(globalThrust * multiplier);
    }

    private void ClampSpeed()
    {
        float maxActual = _boosting ? MaxSpeed * BoostMultiplier : MaxSpeed;
        if (LinearVelocity.LengthSquared() > maxActual * maxActual)
        {
            LinearVelocity = LinearVelocity.Normalized() * maxActual;
        }
    }
}
