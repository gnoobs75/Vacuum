using Godot;
using System.Collections.Generic;
using Vacuum.Systems.Mining;

namespace Vacuum.Systems.AI;

/// <summary>
/// WO-162: Spawns hostile claim jumper NPC ships when the player mines for
/// extended periods. Difficulty scales with distance from star (security zone).
/// Simplified combat: jumpers chase and fire projectiles at the player.
/// </summary>
public partial class ClaimJumperSpawner : Node3D
{
    [Export] public NodePath PlayerShipPath { get; set; } = "";
    [Export] public NodePath MiningLaserPath { get; set; } = "";
    [Export] public string JumperModelPath { get; set; } = "res://assets/models/ships/Miner.glb";
    [Export] public float SpawnCheckInterval { get; set; } = 15f; // check every N seconds while mining
    [Export] public float BaseSpawnChance { get; set; } = 0.15f; // 15% per check
    [Export] public float MinMiningTimeBeforeSpawn { get; set; } = 20f; // seconds of mining before first check
    [Export] public int MaxActiveJumpers { get; set; } = 4;
    [Export] public float SpawnDistance { get; set; } = 150f;

    private Node3D? _playerShip;
    private MiningLaserSystem? _miningLaser;
    private PackedScene? _jumperScene;
    private readonly List<ClaimJumperShip> _activeJumpers = new();
    private float _miningTimer;
    private float _spawnCheckTimer;

    public int ActiveJumperCount => _activeJumpers.Count;

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(PlayerShipPath))
            _playerShip = GetNodeOrNull<Node3D>(PlayerShipPath);
        if (!string.IsNullOrEmpty(MiningLaserPath))
            _miningLaser = GetNodeOrNull<MiningLaserSystem>(MiningLaserPath);

        _jumperScene = GD.Load<PackedScene>(JumperModelPath);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Track mining time
        if (_miningLaser?.IsActive == true)
        {
            _miningTimer += dt;
            _spawnCheckTimer += dt;

            if (_miningTimer >= MinMiningTimeBeforeSpawn && _spawnCheckTimer >= SpawnCheckInterval)
            {
                _spawnCheckTimer = 0f;
                TrySpawnJumper();
            }
        }
        else
        {
            // Slowly decay mining timer when not mining
            _miningTimer = Mathf.Max(0f, _miningTimer - dt * 0.5f);
        }

        // Update active jumpers, remove destroyed ones
        for (int i = _activeJumpers.Count - 1; i >= 0; i--)
        {
            if (!IsInstanceValid(_activeJumpers[i].Node))
            {
                _activeJumpers.RemoveAt(i);
                continue;
            }
            UpdateJumper(_activeJumpers[i], dt);
        }
    }

    private void TrySpawnJumper()
    {
        if (_playerShip == null || _jumperScene == null) return;
        if (_activeJumpers.Count >= MaxActiveJumpers) return;

        float chance = BaseSpawnChance + (_miningTimer / 120f) * 0.1f; // increases over time
        if (GD.Randf() > chance) return;

        // Spawn at random position around player
        var offset = new Vector3(
            (float)GD.RandRange(-1, 1),
            (float)GD.RandRange(-0.3, 0.3),
            (float)GD.RandRange(-1, 1)
        ).Normalized() * SpawnDistance;

        var instance = _jumperScene.Instantiate<Node3D>();
        instance.Scale = Vector3.One * 0.2f;
        instance.GlobalPosition = _playerShip.GlobalPosition + offset;

        // Tint red to indicate hostile
        TintNodeRed(instance);

        AddChild(instance);

        var jumper = new ClaimJumperShip
        {
            Node = instance,
            Health = 100f,
            Speed = (float)GD.RandRange(30, 50),
            FireTimer = 0f,
            FireInterval = (float)GD.RandRange(1.5f, 3f)
        };
        _activeJumpers.Add(jumper);

        GD.Print($"[ClaimJumper] Hostile spawned! ({_activeJumpers.Count} active)");
    }

    private void UpdateJumper(ClaimJumperShip jumper, float dt)
    {
        if (_playerShip == null) return;

        Vector3 toPlayer = _playerShip.GlobalPosition - jumper.Node.GlobalPosition;
        float dist = toPlayer.Length();

        // Chase the player, orbit at close range
        if (dist > 40f)
        {
            jumper.Node.GlobalPosition += toPlayer.Normalized() * jumper.Speed * dt;
        }
        else
        {
            // Orbit around player
            jumper.OrbitAngle += dt * 0.8f;
            Vector3 orbitPos = _playerShip.GlobalPosition + new Vector3(
                Mathf.Cos(jumper.OrbitAngle) * 35f,
                Mathf.Sin(jumper.OrbitAngle * 0.5f) * 10f,
                Mathf.Sin(jumper.OrbitAngle) * 35f
            );
            jumper.Node.GlobalPosition = jumper.Node.GlobalPosition.Lerp(orbitPos, 2f * dt);
        }

        // Face the player
        if (toPlayer.LengthSquared() > 1f)
            jumper.Node.LookAt(_playerShip.GlobalPosition, Vector3.Up);

        // Fire projectiles
        jumper.FireTimer += dt;
        if (jumper.FireTimer >= jumper.FireInterval && dist < 100f)
        {
            jumper.FireTimer = 0f;
            FireProjectile(jumper);
        }
    }

    private void FireProjectile(ClaimJumperShip jumper)
    {
        if (_playerShip == null) return;

        var projectile = new MeshInstance3D();
        var sphere = new SphereMesh { Radius = 0.3f, Height = 0.6f };
        var mat = new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 0.2f, 0.1f),
            EmissionEnabled = true,
            Emission = new Color(1f, 0.3f, 0.1f),
            EmissionEnergyMultiplier = 4f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        sphere.Material = mat;
        projectile.Mesh = sphere;
        projectile.GlobalPosition = jumper.Node.GlobalPosition;

        var script = new ProjectileMover();
        script.Direction = (_playerShip.GlobalPosition - jumper.Node.GlobalPosition).Normalized();
        script.Speed = 80f;
        script.Lifetime = 3f;
        script.Damage = 5f;
        projectile.AddChild(script);

        GetTree().Root.FindChild("Main", true, false)?.AddChild(projectile);
    }

    private void TintNodeRed(Node3D node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
                {
                    var existing = mesh.Mesh?.SurfaceGetMaterial(i) as StandardMaterial3D;
                    if (existing != null)
                    {
                        var tinted = (StandardMaterial3D)existing.Duplicate();
                        tinted.AlbedoColor = new Color(0.8f, 0.15f, 0.1f);
                        mesh.SetSurfaceOverrideMaterial(i, tinted);
                    }
                }
            }
            if (child is Node3D child3d)
                TintNodeRed(child3d);
        }
    }

    private class ClaimJumperShip
    {
        public Node3D Node = null!;
        public float Health;
        public float Speed;
        public float FireTimer;
        public float FireInterval;
        public float OrbitAngle;
    }
}

/// <summary>
/// Simple projectile that moves in a direction and auto-destroys after lifetime.
/// </summary>
public partial class ProjectileMover : Node
{
    public Vector3 Direction;
    public float Speed = 80f;
    public float Lifetime = 3f;
    public float Damage = 5f;

    private float _age;

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _age += dt;
        if (_age >= Lifetime)
        {
            GetParent()?.QueueFree();
            return;
        }

        var parent = GetParentOrNull<Node3D>();
        if (parent != null)
        {
            parent.GlobalPosition += Direction * Speed * dt;
        }
    }
}
