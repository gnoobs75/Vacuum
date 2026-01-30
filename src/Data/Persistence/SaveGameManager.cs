using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Data.Persistence;

/// <summary>
/// Save/load system using Godot's FileAccess for JSON persistence to user data folder.
/// Supports backup rotation.
/// </summary>
public partial class SaveGameManager : Node
{
    public static SaveGameManager? Instance { get; private set; }

    private string _saveDir = "";
    private const int MaxBackups = 3;

    [Signal] public delegate void GameSavedEventHandler(string saveName);
    [Signal] public delegate void GameLoadedEventHandler(string saveName);

    public override void _Ready()
    {
        Instance = this;
        _saveDir = OS.GetUserDataDir() + "/saves";
        if (!DirAccess.DirExistsAbsolute(_saveDir))
            DirAccess.MakeDirRecursiveAbsolute(_saveDir);
    }

    public bool Save<T>(string name, T data) where T : class
    {
        string path = $"{_saveDir}/{name}.json";

        // Rotate backups
        RotateBackups(path);

        try
        {
            string json = GameJsonSerializer.Serialize(data);
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"[SaveGame] Cannot open {path}: {FileAccess.GetOpenError()}");
                return false;
            }
            file.StoreString(json);
            EmitSignal(SignalName.GameSaved, name);
            GD.Print($"[SaveGame] Saved {name}");
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveGame] Save failed: {ex.Message}");
            return false;
        }
    }

    public T? Load<T>(string name) where T : class
    {
        string path = $"{_saveDir}/{name}.json";
        if (!FileAccess.FileExists(path)) return null;

        try
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null) return null;
            string json = file.GetAsText();
            var result = GameJsonSerializer.Deserialize<T>(json);
            if (result != null)
                EmitSignal(SignalName.GameLoaded, name);
            return result;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveGame] Load failed: {ex.Message}");
            return null;
        }
    }

    public List<string> ListSaves()
    {
        var saves = new List<string>();
        using var dir = DirAccess.Open(_saveDir);
        if (dir == null) return saves;

        dir.ListDirBegin();
        string fname;
        while ((fname = dir.GetNext()) != "")
        {
            if (!dir.CurrentIsDir() && fname.EndsWith(".json") && !fname.Contains(".bak"))
                saves.Add(fname.Replace(".json", ""));
        }
        dir.ListDirEnd();
        return saves;
    }

    private void RotateBackups(string path)
    {
        if (!FileAccess.FileExists(path)) return;

        // Shift existing backups
        for (int i = MaxBackups - 1; i >= 1; i--)
        {
            string src = $"{path}.bak{i}";
            string dst = $"{path}.bak{i + 1}";
            if (FileAccess.FileExists(src))
            {
                if (FileAccess.FileExists(dst))
                    DirAccess.RemoveAbsolute(dst);
                DirAccess.RenameAbsolute(src, dst);
            }
        }

        // Current -> .bak1
        string bak1 = $"{path}.bak1";
        if (FileAccess.FileExists(bak1))
            DirAccess.RemoveAbsolute(bak1);
        DirAccess.RenameAbsolute(path, bak1);
    }
}
