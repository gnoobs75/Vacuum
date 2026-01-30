using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services.Caching;

/// <summary>
/// In-memory cache with TTL expiration for frequently accessed game data.
/// </summary>
public partial class CacheManager : Node
{
    public static CacheManager? Instance { get; private set; }

    private readonly Dictionary<string, CacheEntry> _cache = new();
    private double _cleanupTimer;
    private const double CleanupInterval = 30.0;

    public int HitCount { get; private set; }
    public int MissCount { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        _cleanupTimer += delta;
        if (_cleanupTimer >= CleanupInterval)
        {
            _cleanupTimer = 0;
            EvictExpired();
        }
    }

    public void Set<T>(string key, T value, double ttlSeconds = 300) where T : class
    {
        _cache[key] = new CacheEntry
        {
            Value = value!,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ttlSeconds)
        };
    }

    public T? Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
        {
            HitCount++;
            return entry.Value as T;
        }
        MissCount++;
        if (entry != null) _cache.Remove(key); // expired
        return null;
    }

    public bool Has(string key)
    {
        return _cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow;
    }

    public void Invalidate(string key) => _cache.Remove(key);

    public void InvalidateByPrefix(string prefix)
    {
        var toRemove = new List<string>();
        foreach (var key in _cache.Keys)
        {
            if (key.StartsWith(prefix))
                toRemove.Add(key);
        }
        foreach (var key in toRemove)
            _cache.Remove(key);
    }

    public void Clear()
    {
        _cache.Clear();
        HitCount = 0;
        MissCount = 0;
    }

    public float HitRate => (HitCount + MissCount) > 0
        ? (float)HitCount / (HitCount + MissCount) : 0f;

    private void EvictExpired()
    {
        var now = DateTime.UtcNow;
        var toRemove = new List<string>();
        foreach (var (key, entry) in _cache)
        {
            if (entry.ExpiresAt <= now)
                toRemove.Add(key);
        }
        foreach (var key in toRemove)
            _cache.Remove(key);
    }

    private class CacheEntry
    {
        public object Value { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
