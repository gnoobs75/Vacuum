using Godot;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Spawns 2 mining escort ships at each asteroid in the belt.
/// Each miner orbits its asteroid slowly and fires a visual-only mining laser.
/// </summary>
public partial class AsteroidMinerSpawner : Node3D
{
    [Export] public string MinerModelPath { get; set; } = "res://assets/models/ships/Miner.glb";
    [Export] public float MinerScale { get; set; } = 0.25f;
    [Export] public float OrbitRadius { get; set; } = 15f;
    [Export] public float OrbitSpeed { get; set; } = 0.3f;
    [Export] public int MinersPerAsteroid { get; set; } = 2;
    [Export] public NodePath SolarSystemPath { get; set; } = "";

    private PackedScene? _minerScene;
    private float _time;

    private struct AsteroidMiner
    {
        public Node3D Node;
        public Node3D Asteroid;
        public MiningLaserEffect Laser;
        public float AngleOffset;
        public float InclinationOffset;
        public float SpeedVariance;
    }

    private readonly System.Collections.Generic.List<AsteroidMiner> _miners = new();

    public int MinerCount => _miners.Count;

    public override void _Ready()
    {
        _minerScene = GD.Load<PackedScene>(MinerModelPath);
        if (_minerScene == null)
        {
            GD.PrintErr("[AsteroidMinerSpawner] Failed to load miner model.");
            return;
        }

        // Defer so the solar system has time to generate
        CallDeferred(MethodName.SpawnMinersAtAsteroids);
    }

    private void SpawnMinersAtAsteroids()
    {
        Node3D? solarSystem = null;
        if (!string.IsNullOrEmpty(SolarSystemPath))
            solarSystem = GetNodeOrNull<Node3D>(SolarSystemPath);

        if (solarSystem == null) return;

        // Find the AsteroidBelt node
        var belt = solarSystem.GetNodeOrNull<Node3D>("AsteroidBelt");
        if (belt == null)
        {
            GD.PrintErr("[AsteroidMinerSpawner] AsteroidBelt node not found.");
            return;
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        int asteroidIndex = 0;
        foreach (var child in belt.GetChildren())
        {
            if (child is not Node3D asteroid) continue;

            for (int m = 0; m < MinersPerAsteroid; m++)
            {
                var instance = _minerScene!.Instantiate<Node3D>();
                instance.Scale = Vector3.One * MinerScale;

                float angleOff = (Mathf.Tau / MinersPerAsteroid) * m + rng.RandfRange(0, 0.5f);
                float incOff = rng.RandfRange(0.3f, Mathf.Pi - 0.3f);

                // Initial position near asteroid
                instance.GlobalPosition = asteroid.GlobalPosition + new Vector3(
                    Mathf.Sin(incOff) * Mathf.Cos(angleOff) * OrbitRadius,
                    Mathf.Cos(incOff) * OrbitRadius,
                    Mathf.Sin(incOff) * Mathf.Sin(angleOff) * OrbitRadius
                );

                AddChild(instance);

                // Create mining laser effect
                var laser = new MiningLaserEffect();
                // Vary laser colors slightly per miner
                laser.BeamColor = m % 2 == 0
                    ? new Color(0.2f, 0.8f, 1.0f)
                    : new Color(0.4f, 1.0f, 0.6f);
                laser.Initialize(instance, asteroid);
                AddChild(laser);

                _miners.Add(new AsteroidMiner
                {
                    Node = instance,
                    Asteroid = asteroid,
                    Laser = laser,
                    AngleOffset = angleOff,
                    InclinationOffset = incOff,
                    SpeedVariance = rng.RandfRange(-0.1f, 0.1f)
                });
            }

            asteroidIndex++;
        }

        GD.Print($"[AsteroidMinerSpawner] Spawned {_miners.Count} miners at {asteroidIndex} asteroids.");
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _time += dt;

        for (int i = 0; i < _miners.Count; i++)
        {
            var miner = _miners[i];
            if (!IsInstanceValid(miner.Node) || !IsInstanceValid(miner.Asteroid)) continue;

            float angle = _time * (OrbitSpeed + miner.SpeedVariance) + miner.AngleOffset;
            float inc = miner.InclinationOffset;

            Vector3 orbitPos = miner.Asteroid.GlobalPosition + new Vector3(
                Mathf.Sin(inc) * Mathf.Cos(angle) * OrbitRadius,
                Mathf.Cos(inc) * OrbitRadius,
                Mathf.Sin(inc) * Mathf.Sin(angle) * OrbitRadius
            );

            miner.Node.GlobalPosition = miner.Node.GlobalPosition.Lerp(orbitPos, 4f * dt);

            // Face the asteroid
            Vector3 toAsteroid = miner.Asteroid.GlobalPosition - miner.Node.GlobalPosition;
            if (toAsteroid.LengthSquared() > 0.01f)
            {
                miner.Node.LookAt(miner.Asteroid.GlobalPosition, Vector3.Up);
            }
        }
    }
}
