using Godot;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Visual-only mining laser beam from a ship to its target asteroid.
/// Renders a glowing beam line and particle sparks at the impact point.
/// </summary>
public partial class MiningLaserEffect : Node3D
{
    [Export] public Color BeamColor { get; set; } = new Color(0.2f, 0.8f, 1.0f);
    [Export] public float BeamWidth { get; set; } = 0.15f;
    [Export] public float PulseSpeed { get; set; } = 4f;

    private Node3D? _source;
    private Node3D? _target;
    private MeshInstance3D? _beamMesh;
    private GpuParticles3D? _sparks;
    private OmniLight3D? _impactLight;
    private float _time;

    public void Initialize(Node3D source, Node3D target)
    {
        _source = source;
        _target = target;
    }

    public override void _Ready()
    {
        CreateBeam();
        CreateSparks();
        CreateImpactLight();
    }

    public override void _Process(double delta)
    {
        if (_source == null || _target == null || !IsInstanceValid(_source) || !IsInstanceValid(_target))
            return;

        _time += (float)delta;

        Vector3 from = _source.GlobalPosition;
        Vector3 to = _target.GlobalPosition;
        Vector3 mid = (from + to) * 0.5f;
        float dist = from.DistanceTo(to);

        // Position beam at midpoint, oriented along the line
        if (_beamMesh != null)
        {
            _beamMesh.GlobalPosition = mid;
            _beamMesh.LookAt(to, Vector3.Up);

            // Scale the cylinder to span the distance
            _beamMesh.Scale = new Vector3(BeamWidth, BeamWidth, dist);

            // Pulse the beam brightness
            float pulse = 0.7f + 0.3f * Mathf.Sin(_time * PulseSpeed);
            var mat = _beamMesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
            if (mat != null)
            {
                mat.EmissionEnergyMultiplier = 2f * pulse;
                mat.AlbedoColor = BeamColor with { A = 0.6f * pulse };
            }
        }

        // Sparks at impact point
        if (_sparks != null)
        {
            _sparks.GlobalPosition = to;
            _sparks.Emitting = true;
        }

        // Light at impact
        if (_impactLight != null)
        {
            _impactLight.GlobalPosition = to;
            float pulse = 0.7f + 0.3f * Mathf.Sin(_time * PulseSpeed * 2f);
            _impactLight.LightEnergy = 1.5f * pulse;
        }
    }

    private void CreateBeam()
    {
        _beamMesh = new MeshInstance3D();

        // Use a cylinder mesh oriented along Z
        var cylinder = new CylinderMesh
        {
            TopRadius = 0.5f,
            BottomRadius = 0.5f,
            Height = 1f,
            RadialSegments = 6,
            Rings = 1
        };

        // Rotate the mesh so it extends along Z instead of Y
        _beamMesh.Mesh = cylinder;
        _beamMesh.RotateObjectLocal(Vector3.Right, Mathf.Pi / 2f);

        var mat = new StandardMaterial3D
        {
            AlbedoColor = BeamColor with { A = 0.6f },
            EmissionEnabled = true,
            Emission = BeamColor,
            EmissionEnergyMultiplier = 2f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            NoDepthTest = true
        };
        _beamMesh.SetSurfaceOverrideMaterial(0, mat);

        // Wrap in a pivot so the rotation trick works with LookAt
        var pivot = new Node3D { Name = "BeamPivot" };
        pivot.AddChild(_beamMesh);
        AddChild(pivot);

        // Re-assign so _Process controls the pivot
        // Actually, simplify: put the mesh directly, handle orientation in _Process
        pivot.RemoveChild(_beamMesh);
        pivot.QueueFree();
        AddChild(_beamMesh);
    }

    private void CreateSparks()
    {
        _sparks = new GpuParticles3D();
        _sparks.Amount = 24;
        _sparks.Lifetime = 0.6f;
        _sparks.Explosiveness = 0.1f;
        _sparks.SpeedScale = 1.5f;

        var particleMat = new ParticleProcessMaterial();
        particleMat.Direction = new Vector3(0, 1, 0);
        particleMat.Spread = 60f;
        particleMat.InitialVelocityMin = 2f;
        particleMat.InitialVelocityMax = 6f;
        particleMat.Gravity = Vector3.Zero;
        particleMat.ScaleMin = 0.3f;
        particleMat.ScaleMax = 0.8f;
        particleMat.Color = new Color(0.3f, 0.9f, 1.0f);
        _sparks.ProcessMaterial = particleMat;

        // Small sphere for each spark particle
        var sparkMesh = new SphereMesh
        {
            Radius = 0.08f,
            Height = 0.16f,
            RadialSegments = 4,
            Rings = 2
        };
        var sparkDrawMat = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.5f, 0.9f, 1.0f),
            EmissionEnabled = true,
            Emission = new Color(0.3f, 0.8f, 1.0f),
            EmissionEnergyMultiplier = 3f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        sparkMesh.Material = sparkDrawMat;
        _sparks.DrawPass1 = sparkMesh;

        AddChild(_sparks);
    }

    private void CreateImpactLight()
    {
        _impactLight = new OmniLight3D
        {
            LightColor = BeamColor,
            LightEnergy = 1.5f,
            OmniRange = 8f,
            ShadowEnabled = false
        };
        AddChild(_impactLight);
    }
}
