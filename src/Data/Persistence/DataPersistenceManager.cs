using System;
using Godot;

namespace Vacuum.Data.Persistence;

/// <summary>
/// High-level persistence manager coordinating save/load operations.
/// </summary>
public partial class DataPersistenceManager : Node
{
    public static DataPersistenceManager? Instance { get; private set; }

    private string _dataDir = "";

    public override void _Ready()
    {
        Instance = this;
        _dataDir = OS.GetUserDataDir() + "/data";
        if (!DirAccess.DirExistsAbsolute(_dataDir))
            DirAccess.MakeDirRecursiveAbsolute(_dataDir);
    }

    public bool PersistData<T>(string collection, string id, T data) where T : class
    {
        string dir = $"{_dataDir}/{collection}";
        if (!DirAccess.DirExistsAbsolute(dir))
            DirAccess.MakeDirRecursiveAbsolute(dir);

        string path = $"{dir}/{id}.json";
        try
        {
            string json = GameJsonSerializer.Serialize(data);
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file == null) return false;
            file.StoreString(json);
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DataPersistence] Write failed: {ex.Message}");
            return false;
        }
    }

    public T? LoadData<T>(string collection, string id) where T : class
    {
        string path = $"{_dataDir}/{collection}/{id}.json";
        if (!FileAccess.FileExists(path)) return null;

        try
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null) return null;
            return GameJsonSerializer.Deserialize<T>(file.GetAsText());
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DataPersistence] Read failed: {ex.Message}");
            return null;
        }
    }

    public bool DeleteData(string collection, string id)
    {
        string path = $"{_dataDir}/{collection}/{id}.json";
        if (!FileAccess.FileExists(path)) return false;
        return DirAccess.RemoveAbsolute(path) == Error.Ok;
    }
}
