using System.Collections.Generic;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Game configuration management. Reads from Godot project settings and provides
/// runtime configuration access.
/// </summary>
public partial class ConfigurationManager : BaseService
{
    public static ConfigurationManager? Instance { get; private set; }

    private readonly Dictionary<string, Variant> _overrides = new();

    public bool IsDebug { get; private set; }

    protected override void InitializeService()
    {
        Instance = this;
        IsDebug = OS.IsDebugBuild();
    }

    public T GetSetting<T>(string path, T defaultValue)
    {
        if (_overrides.TryGetValue(path, out var over))
        {
            if (over.Obj is T val)
                return val;
        }

        if (ProjectSettings.HasSetting(path))
        {
            var v = ProjectSettings.GetSetting(path);
            if (v.Obj is T result)
                return result;
        }

        return defaultValue;
    }

    public void SetOverride(string path, Variant value)
    {
        _overrides[path] = value;
    }

    public void ClearOverride(string path) => _overrides.Remove(path);
}
