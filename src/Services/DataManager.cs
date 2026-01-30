using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Singleton service for in-memory data storage and JSON persistence to user data folder.
/// </summary>
public partial class DataManager : BaseService
{
    public static DataManager? Instance { get; private set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly Dictionary<string, object> _store = new();
    private string _saveDirectory = "";

    protected override void InitializeService()
    {
        Instance = this;
        _saveDirectory = OS.GetUserDataDir() + "/saves";
        EnsureDirectoryExists(_saveDirectory);
    }

    /// <summary>Store a data object by key.</summary>
    public void Set<T>(string key, T data) where T : class
    {
        _store[key] = data!;
    }

    /// <summary>Retrieve a data object by key.</summary>
    public T? Get<T>(string key) where T : class
    {
        return _store.TryGetValue(key, out var obj) ? obj as T : null;
    }

    public bool Has(string key) => _store.ContainsKey(key);
    public void Remove(string key) => _store.Remove(key);

    /// <summary>Save a data object to JSON file.</summary>
    public bool SaveToFile<T>(string filename, T data) where T : class
    {
        try
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);
            string path = $"{_saveDirectory}/{filename}";
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                LogError($"Could not open {path} for writing: {FileAccess.GetOpenError()}");
                return false;
            }
            file.StoreString(json);
            Log($"Saved {filename}");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Save failed ({filename}): {ex.Message}");
            return false;
        }
    }

    /// <summary>Load a data object from JSON file.</summary>
    public T? LoadFromFile<T>(string filename) where T : class
    {
        string path = $"{_saveDirectory}/{filename}";
        if (!FileAccess.FileExists(path))
            return null;

        try
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null) return null;
            string json = file.GetAsText();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            LogError($"Load failed ({filename}): {ex.Message}");
            return null;
        }
    }

    /// <summary>List all save files matching a pattern.</summary>
    public List<string> ListSaveFiles(string extension = ".json")
    {
        var files = new List<string>();
        using var dir = DirAccess.Open(_saveDirectory);
        if (dir == null) return files;

        dir.ListDirBegin();
        string name;
        while ((name = dir.GetNext()) != "")
        {
            if (!dir.CurrentIsDir() && name.EndsWith(extension))
                files.Add(name);
        }
        dir.ListDirEnd();
        return files;
    }

    public bool DeleteSaveFile(string filename)
    {
        string path = $"{_saveDirectory}/{filename}";
        if (!FileAccess.FileExists(path)) return false;
        return DirAccess.RemoveAbsolute(path) == Error.Ok;
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!DirAccess.DirExistsAbsolute(path))
            DirAccess.MakeDirRecursiveAbsolute(path);
    }
}
