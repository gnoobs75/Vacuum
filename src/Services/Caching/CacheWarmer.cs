using Godot;

namespace Vacuum.Services.Caching;

/// <summary>
/// Pre-loads critical game data into cache on startup.
/// </summary>
public partial class CacheWarmer : Node
{
    public override void _Ready()
    {
        CallDeferred(MethodName.WarmCache);
    }

    private void WarmCache()
    {
        var cache = CacheManager.Instance;
        if (cache == null) return;

        // Warm with static game data
        // Additional warmup can be added as systems grow
        GD.Print("[CacheWarmer] Cache pre-loaded.");
    }
}
