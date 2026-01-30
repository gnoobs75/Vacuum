using System;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Centralized error handler. Services route exceptions here for consistent
/// logging and optional signal-based notification.
/// </summary>
public partial class GlobalErrorHandler : Node
{
    public static GlobalErrorHandler? Instance { get; private set; }

    [Signal] public delegate void ErrorOccurredEventHandler(string service, string message, string severity);

    public override void _Ready()
    {
        Instance = this;
    }

    public void HandleError(string service, Exception ex, ErrorSeverity severity = ErrorSeverity.Error)
    {
        string msg = $"[{service}] {severity}: {ex.Message}";
        switch (severity)
        {
            case ErrorSeverity.Warning:
                GD.PushWarning(msg);
                break;
            case ErrorSeverity.Critical:
                GD.PrintErr(msg);
                GD.PrintErr(ex.StackTrace ?? "");
                break;
            default:
                GD.PrintErr(msg);
                break;
        }
        EmitSignal(SignalName.ErrorOccurred, service, ex.Message, severity.ToString());
    }

    public void HandleError(string service, string message, ErrorSeverity severity = ErrorSeverity.Error)
    {
        HandleError(service, new Exception(message), severity);
    }
}

public enum ErrorSeverity
{
    Warning,
    Error,
    Critical
}
