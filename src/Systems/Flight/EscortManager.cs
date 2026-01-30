using Godot;
using System.Collections.Generic;

namespace Vacuum.Systems.Flight;

/// <summary>
/// Manages escort ships that orbit the player ship in varied 3D patterns,
/// forming a sphere-like cloud as numbers increase. Press N to spawn 3 more.
/// </summary>
public partial class EscortManager : Node3D
{
    [Export] public NodePath PlayerShipPath { get; set; } = "";
    [Export] public string EscortModelPath { get; set; } = "res://assets/models/ships/Miner.glb";
    [Export] public float OrbitRadius { get; set; } = 30f;
    [Export] public float OrbitSpeed { get; set; } = 0.5f;
    [Export] public float EscortScale { get; set; } = 0.25f;
    [Export] public float FollowSmoothSpeed { get; set; } = 3f;
    [Export] public int EscortsPerSpawn { get; set; } = 3;

    private Node3D? _playerShip;
    private PackedScene? _escortScene;
    private readonly List<EscortShip> _escorts = new();
    private float _time;

    public int EscortCount => _escorts.Count;

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(PlayerShipPath))
            _playerShip = GetNodeOrNull<Node3D>(PlayerShipPath);

        _escortScene = GD.Load<PackedScene>(EscortModelPath);

        SpawnEscorts(EscortsPerSpawn);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.N)
        {
            SpawnEscorts(EscortsPerSpawn);
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (_playerShip == null) return;

        float dt = (float)delta;
        _time += dt;

        Vector3 playerPos = _playerShip.GlobalPosition;
        Vector3 playerUp = _playerShip.GlobalTransform.Basis.Y;

        for (int i = 0; i < _escorts.Count; i++)
        {
            var escort = _escorts[i];
            if (!IsInstanceValid(escort.Node)) continue;

            // Use golden angle distribution on a sphere so each escort gets a
            // unique inclination, and they naturally fill a sphere as count grows.
            float goldenAngle = Mathf.Pi * (3f - Mathf.Sqrt(5f)); // ~2.399 rad
            float inclination = Mathf.Acos(1f - 2f * (i + 0.5f) / Mathf.Max(_escorts.Count, 1));
            float azimuth = goldenAngle * i;

            // Animate: each escort orbits at its own inclination ring
            float animAngle = _time * (OrbitSpeed + escort.SpeedVariance) + azimuth;

            // Spherical to cartesian (inclination = polar angle from +Y)
            float sinInc = Mathf.Sin(inclination);
            float cosInc = Mathf.Cos(inclination);
            float r = OrbitRadius + escort.RadiusVariance;

            Vector3 localPos = new Vector3(
                sinInc * Mathf.Cos(animAngle) * r,
                cosInc * r,
                sinInc * Mathf.Sin(animAngle) * r
            );

            Vector3 orbitPos = playerPos + localPos;

            // Smooth follow
            escort.Node.GlobalPosition = escort.Node.GlobalPosition.Lerp(orbitPos, FollowSmoothSpeed * dt);

            // Face tangent direction (derivative of position w.r.t. animAngle)
            Vector3 tangent = new Vector3(
                sinInc * -Mathf.Sin(animAngle),
                0f,
                sinInc * Mathf.Cos(animAngle)
            );
            if (tangent.LengthSquared() > 0.001f)
            {
                escort.Node.LookAt(escort.Node.GlobalPosition + tangent, playerUp);
            }
        }
    }

    public void SpawnEscorts(int count)
    {
        if (_escortScene == null) return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        for (int i = 0; i < count; i++)
        {
            var instance = _escortScene.Instantiate<Node3D>();
            instance.Scale = Vector3.One * EscortScale;

            if (_playerShip != null)
            {
                instance.GlobalPosition = _playerShip.GlobalPosition + new Vector3(
                    (float)GD.RandRange(-OrbitRadius, OrbitRadius),
                    (float)GD.RandRange(-OrbitRadius, OrbitRadius),
                    (float)GD.RandRange(-OrbitRadius, OrbitRadius)
                );
            }

            AddChild(instance);
            _escorts.Add(new EscortShip
            {
                Node = instance,
                SpeedVariance = rng.RandfRange(-0.15f, 0.15f),
                RadiusVariance = rng.RandfRange(-5f, 5f)
            });
        }
    }

    private struct EscortShip
    {
        public Node3D Node;
        public float SpeedVariance;
        public float RadiusVariance;
    }
}
