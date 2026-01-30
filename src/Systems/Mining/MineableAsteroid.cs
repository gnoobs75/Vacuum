using Godot;
using Vacuum.Data;
using Vacuum.Data.Models;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Component attached to asteroid nodes to make them mineable.
/// Tracks ore type, remaining quantity, and respawn after depletion.
/// </summary>
public partial class MineableAsteroid : Node
{
    [Export] public float RespawnTime { get; set; } = 120f; // seconds to respawn after depletion

    public AsteroidState State { get; private set; } = new();

    private MeshInstance3D? _parentMesh;
    private float _originalScale;
    private bool _respawning;
    private float _respawnTimer;

    public void Initialize(string oreId, float totalOre)
    {
        State = new AsteroidState
        {
            OreId = oreId,
            TotalOre = totalOre,
            RemainingOre = totalOre
        };
    }

    public override void _Ready()
    {
        _parentMesh = GetParentOrNull<MeshInstance3D>();
        if (_parentMesh != null)
            _originalScale = _parentMesh.Scale.X;
    }

    public override void _Process(double delta)
    {
        if (!_respawning && State.Depleted)
        {
            _respawning = true;
            _respawnTimer = 0f;
            // Visually shrink the asteroid
            if (_parentMesh != null)
                _parentMesh.Visible = false;
        }

        if (_respawning)
        {
            _respawnTimer += (float)delta;
            if (_respawnTimer >= RespawnTime)
            {
                // Respawn with full ore
                State.RemainingOre = State.TotalOre;
                _respawning = false;
                if (_parentMesh != null)
                    _parentMesh.Visible = true;
            }
        }
        else if (_parentMesh != null && State.TotalOre > 0)
        {
            // Scale asteroid based on remaining ore
            float fraction = State.RemainingOre / State.TotalOre;
            float scale = Mathf.Lerp(_originalScale * 0.3f, _originalScale, fraction);
            _parentMesh.Scale = Vector3.One * scale;
        }
    }

    /// <summary>
    /// Get a display color tint based on ore rarity.
    /// </summary>
    public Color GetOreColor()
    {
        if (!OreDatabase.Ores.TryGetValue(State.OreId, out var def))
            return new Color(0.55f, 0.5f, 0.45f);

        return def.Rarity switch
        {
            OreRarity.Common => new Color(0.55f, 0.5f, 0.45f),
            OreRarity.Uncommon => new Color(0.4f, 0.6f, 0.5f),
            OreRarity.Moderate => new Color(0.5f, 0.5f, 0.7f),
            OreRarity.Rare => new Color(0.7f, 0.5f, 0.3f),
            OreRarity.VeryRare => new Color(0.8f, 0.4f, 0.8f),
            OreRarity.ExtremelyRare => new Color(1f, 0.8f, 0.2f),
            _ => new Color(0.55f, 0.5f, 0.45f)
        };
    }
}
