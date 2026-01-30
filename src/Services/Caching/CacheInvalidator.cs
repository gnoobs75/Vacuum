using Godot;

namespace Vacuum.Services.Caching;

/// <summary>
/// Manages cache invalidation on data changes. Connect to relevant signals
/// to auto-invalidate stale cache entries.
/// </summary>
public partial class CacheInvalidator : Node
{
    public override void _Ready()
    {
        GD.Print("[CacheInvalidator] Ready.");
    }

    /// <summary>Invalidate cache entries related to a specific entity type.</summary>
    public void InvalidateEntity(string entityType, string entityId)
    {
        CacheManager.Instance?.Invalidate($"{entityType}:{entityId}");
        CacheManager.Instance?.InvalidateByPrefix($"{entityType}:list");
    }

    /// <summary>Invalidate all market-related cache entries.</summary>
    public void InvalidateMarket(string stationId)
    {
        CacheManager.Instance?.InvalidateByPrefix($"market:{stationId}");
    }
}
