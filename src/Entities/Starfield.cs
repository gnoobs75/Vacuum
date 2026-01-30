using Godot;

namespace Vacuum.Entities;

/// <summary>
/// Procedural starfield rendered on a large sphere that follows the camera position.
/// Stars rotate with the camera FOV (parallax-free at infinity) because the sphere
/// is centered on the camera but never rotates with it.
/// Uses MultiMeshInstance3D for performance with thousands of star points.
/// </summary>
public partial class Starfield : Node3D
{
    [Export] public int StarCount { get; set; } = 3000;
    [Export] public float SphereRadius { get; set; } = 9000f;
    [Export] public float MinStarSize { get; set; } = 0.5f;
    [Export] public float MaxStarSize { get; set; } = 3f;

    private Camera3D? _camera;

    public override void _Ready()
    {
        GenerateStars();
        // Find camera after tree is ready
        CallDeferred(MethodName.FindCamera);
    }

    private void FindCamera()
    {
        _camera = GetViewport().GetCamera3D();
    }

    public override void _Process(double delta)
    {
        // Follow camera position so stars always appear infinitely far
        if (_camera != null)
        {
            GlobalPosition = _camera.GlobalPosition;
        }
    }

    private void GenerateStars()
    {
        // Use a MultiMesh for efficient rendering of many small quads
        var multiMesh = new MultiMesh();
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.UseColors = true;
        multiMesh.InstanceCount = StarCount;

        // Each star is a small quad (plane mesh facing the center)
        var quadMesh = new QuadMesh();
        quadMesh.Size = new Vector2(1f, 1f);
        var starMat = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            VertexColorUseAsAlbedo = true,
            EmissionEnabled = true,
            Emission = Colors.White,
            EmissionEnergyMultiplier = 2f,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            NoDepthTest = true,
            RenderPriority = -100
        };
        quadMesh.Material = starMat;
        multiMesh.Mesh = quadMesh;

        for (int i = 0; i < StarCount; i++)
        {
            // Random point on sphere surface
            float theta = (float)GD.RandRange(0, Mathf.Tau);
            float phi = Mathf.Acos((float)GD.RandRange(-1.0, 1.0));

            float x = SphereRadius * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = SphereRadius * Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = SphereRadius * Mathf.Cos(phi);

            float size = (float)GD.RandRange(MinStarSize, MaxStarSize);

            var transform = Transform3D.Identity;
            transform = transform.Scaled(new Vector3(size, size, size));
            transform.Origin = new Vector3(x, y, z);
            multiMesh.SetInstanceTransform(i, transform);

            // Vary star color slightly - mostly white with some blue/yellow tints
            float colorVariant = (float)GD.RandRange(0.0, 1.0);
            Color starColor;
            if (colorVariant < 0.6f)
            {
                // White stars
                float brightness = (float)GD.RandRange(0.7, 1.0);
                starColor = new Color(brightness, brightness, brightness + 0.05f);
            }
            else if (colorVariant < 0.8f)
            {
                // Blue-white stars
                float b = (float)GD.RandRange(0.8, 1.0);
                starColor = new Color(b * 0.8f, b * 0.85f, b);
            }
            else if (colorVariant < 0.93f)
            {
                // Yellow stars
                float b = (float)GD.RandRange(0.7, 1.0);
                starColor = new Color(b, b * 0.9f, b * 0.6f);
            }
            else
            {
                // Red/orange stars (rare)
                float b = (float)GD.RandRange(0.6, 0.9);
                starColor = new Color(b, b * 0.5f, b * 0.3f);
            }

            multiMesh.SetInstanceColor(i, starColor);
        }

        var instance = new MultiMeshInstance3D();
        instance.Multimesh = multiMesh;
        instance.Name = "StarfieldMesh";
        AddChild(instance);
    }
}
