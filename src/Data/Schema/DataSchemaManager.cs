using System.Collections.Generic;
using Godot;

namespace Vacuum.Data.Schema;

/// <summary>
/// Manages JSON schema versioning for data migration and compatibility.
/// </summary>
public partial class DataSchemaManager : Node
{
    public static DataSchemaManager? Instance { get; private set; }

    public int CurrentVersion { get; } = 1;

    private readonly Dictionary<string, int> _schemaVersions = new();

    public override void _Ready()
    {
        Instance = this;
        RegisterSchema("save_game", 1);
        RegisterSchema("player_data", 1);
        RegisterSchema("market_data", 1);
    }

    public void RegisterSchema(string name, int version)
    {
        _schemaVersions[name] = version;
    }

    public int GetSchemaVersion(string name)
    {
        return _schemaVersions.TryGetValue(name, out var v) ? v : 0;
    }

    public bool NeedsMigration(string name, int fileVersion)
    {
        return fileVersion < GetSchemaVersion(name);
    }
}
