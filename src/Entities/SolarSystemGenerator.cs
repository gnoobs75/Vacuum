using Godot;

namespace Vacuum.Entities;

/// <summary>
/// Procedurally generates a basic solar system with star, planets, stations, and asteroid belt.
/// </summary>
public partial class SolarSystemGenerator : Node3D
{
    [Export] public int PlanetCount { get; set; } = 6;
    [Export] public int AsteroidCount { get; set; } = 200;
    [Export] public int StationCount { get; set; } = 3;
    [Export] public float InnerOrbitRadius { get; set; } = 500f;
    [Export] public float OuterOrbitRadius { get; set; } = 3000f;
    [Export] public float AsteroidBeltRadius { get; set; } = 1500f;
    [Export] public float AsteroidBeltWidth { get; set; } = 300f;

    public override void _Ready()
    {
        GenerateEnvironment();
        GenerateStar();
        GeneratePlanets();
        GenerateAsteroidBelt();
        GenerateStations();
        GD.Print("[SolarSystem] Generation complete.");
    }

    private void GenerateEnvironment()
    {
        var env = new WorldEnvironment();
        var envRes = new Godot.Environment();

        // Dark space background
        envRes.BackgroundMode = Godot.Environment.BGMode.Color;
        envRes.BackgroundColor = new Color(0.005f, 0.005f, 0.015f);

        // Ambient light so objects are always somewhat visible even in shadow
        envRes.AmbientLightSource = Godot.Environment.AmbientSource.Color;
        envRes.AmbientLightColor = new Color(0.15f, 0.15f, 0.25f);
        envRes.AmbientLightEnergy = 0.8f;

        // Tonemap for better dynamic range
        envRes.TonemapMode = Godot.Environment.ToneMapper.Aces;
        envRes.TonemapExposure = 1.2f;

        // Glow for emissive objects (star, engines)
        envRes.GlowEnabled = true;
        envRes.GlowIntensity = 0.6f;
        envRes.GlowBloom = 0.3f;

        env.Environment = envRes;
        env.Name = "WorldEnvironment";
        AddChild(env);
    }

    private void GenerateStar()
    {
        var starNode = new Node3D { Name = "Star" };

        // Point light from the star - high range, high energy
        var pointLight = new OmniLight3D
        {
            OmniRange = 8000f,
            LightEnergy = 4f,
            LightColor = new Color(1f, 0.95f, 0.85f),
            ShadowEnabled = false // performance
        };
        starNode.AddChild(pointLight);

        // Directional light simulating sunlight across the whole system
        var dirLight = new DirectionalLight3D
        {
            LightEnergy = 1.5f,
            LightColor = new Color(1f, 0.97f, 0.9f),
            ShadowEnabled = true,
            RotationDegrees = new Vector3(-30f, -45f, 0f)
        };
        dirLight.Name = "SunDirectional";
        AddChild(dirLight);

        // Visual sphere for the star
        var mesh = new MeshInstance3D();
        var sphere = new SphereMesh { Radius = 50f, Height = 100f };
        var mat = new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 0.9f, 0.5f),
            EmissionEnabled = true,
            Emission = new Color(1f, 0.85f, 0.4f),
            EmissionEnergyMultiplier = 8f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        sphere.Material = mat;
        mesh.Mesh = sphere;
        starNode.AddChild(mesh);

        AddChild(starNode);
    }

    private void GeneratePlanets()
    {
        for (int i = 0; i < PlanetCount; i++)
        {
            float t = (float)i / PlanetCount;
            float radius = Mathf.Lerp(InnerOrbitRadius, OuterOrbitRadius, t);
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float size = (float)GD.RandRange(10, 40);

            var planet = new Node3D { Name = $"Planet_{i}" };
            var mesh = new MeshInstance3D();
            var sphere = new SphereMesh
            {
                Radius = size,
                Height = size * 2f,
                RadialSegments = 32,
                Rings = 16
            };
            var mat = new StandardMaterial3D
            {
                AlbedoColor = new Color(
                    (float)GD.RandRange(0.4, 1.0),
                    (float)GD.RandRange(0.4, 1.0),
                    (float)GD.RandRange(0.4, 1.0)),
                Roughness = 0.7f,
                Metallic = 0.1f
            };
            sphere.Material = mat;
            mesh.Mesh = sphere;

            planet.Position = new Vector3(
                Mathf.Cos(angle) * radius,
                (float)GD.RandRange(-50, 50),
                Mathf.Sin(angle) * radius
            );

            var body = new StaticBody3D();
            var collider = new CollisionShape3D();
            collider.Shape = new SphereShape3D { Radius = size };
            body.AddChild(collider);

            planet.AddChild(mesh);
            planet.AddChild(body);
            AddChild(planet);
        }
    }

    private void GenerateAsteroidBelt()
    {
        var beltNode = new Node3D { Name = "AsteroidBelt" };

        for (int i = 0; i < AsteroidCount; i++)
        {
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float dist = AsteroidBeltRadius + (float)GD.RandRange(-AsteroidBeltWidth / 2, AsteroidBeltWidth / 2);
            float size = (float)GD.RandRange(1, 8);

            var asteroid = new MeshInstance3D { Name = $"Asteroid_{i}" };
            var box = new BoxMesh
            {
                Size = new Vector3(size, size * 0.7f, size * 0.8f)
            };
            var mat = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.55f, 0.5f, 0.45f),
                Roughness = 0.9f,
                Metallic = 0.2f
            };
            box.Material = mat;
            asteroid.Mesh = box;

            asteroid.Position = new Vector3(
                Mathf.Cos(angle) * dist,
                (float)GD.RandRange(-40, 40),
                Mathf.Sin(angle) * dist
            );
            asteroid.RotationDegrees = new Vector3(
                (float)GD.RandRange(0, 360),
                (float)GD.RandRange(0, 360),
                (float)GD.RandRange(0, 360)
            );

            beltNode.AddChild(asteroid);
        }

        AddChild(beltNode);
    }

    private void GenerateStations()
    {
        for (int i = 0; i < StationCount; i++)
        {
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float dist = (float)GD.RandRange(InnerOrbitRadius * 0.8f, OuterOrbitRadius * 0.6f);

            var station = new Node3D { Name = $"Station_{i}" };
            var mesh = new MeshInstance3D();
            var box = new BoxMesh { Size = new Vector3(30, 15, 30) };
            var mat = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.7f, 0.7f, 0.8f),
                Metallic = 0.8f,
                Roughness = 0.3f
            };
            box.Material = mat;
            mesh.Mesh = box;

            station.Position = new Vector3(
                Mathf.Cos(angle) * dist,
                (float)GD.RandRange(-20, 20),
                Mathf.Sin(angle) * dist
            );

            var body = new StaticBody3D();
            var collider = new CollisionShape3D();
            collider.Shape = new BoxShape3D { Size = new Vector3(30, 15, 30) };
            body.AddChild(collider);

            station.AddChild(mesh);
            station.AddChild(body);
            AddChild(station);
        }
    }
}
