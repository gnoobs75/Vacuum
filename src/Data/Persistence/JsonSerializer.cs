using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Vacuum.Data.Persistence;

/// <summary>
/// JSON serialization utility with Godot-compatible error handling.
/// </summary>
public static class GameJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException ex)
        {
            GD.PrintErr($"[JsonSerializer] Deserialize failed: {ex.Message}");
            return default;
        }
    }

    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json, Options);
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
