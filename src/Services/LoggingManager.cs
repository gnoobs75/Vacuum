using System;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Structured logging with categories and levels.
/// </summary>
public partial class LoggingManager : BaseService
{
    public static LoggingManager? Instance { get; private set; }

    public enum LogLevel { Debug, Info, Warning, Error }
    public LogLevel MinLevel { get; set; } = LogLevel.Info;

    protected override void InitializeService()
    {
        Instance = this;
        if (OS.IsDebugBuild())
            MinLevel = LogLevel.Debug;
    }

    public void LogDebug(string category, string message)
    {
        if (MinLevel <= LogLevel.Debug)
            GD.Print($"[DEBUG][{category}] {message}");
    }

    public void LogInfo(string category, string message)
    {
        if (MinLevel <= LogLevel.Info)
            GD.Print($"[{category}] {message}");
    }

    public void LogWarn(string category, string message)
    {
        if (MinLevel <= LogLevel.Warning)
            GD.PushWarning($"[{category}] {message}");
    }

    public void LogErr(string category, string message)
    {
        GD.PrintErr($"[{category}] {message}");
    }
}
