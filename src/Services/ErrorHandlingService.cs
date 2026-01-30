using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Centralized error tracking and reporting service.
/// </summary>
public partial class ErrorHandlingService : BaseService
{
    public static ErrorHandlingService? Instance { get; private set; }

    private readonly List<ErrorRecord> _recentErrors = new();
    private const int MaxErrorHistory = 100;

    [Signal] public delegate void ErrorLoggedEventHandler(string service, string message);

    protected override void InitializeService()
    {
        Instance = this;
    }

    public void LogError(string service, Exception ex)
    {
        var record = new ErrorRecord
        {
            Service = service,
            Message = ex.Message,
            StackTrace = ex.StackTrace ?? "",
            Timestamp = DateTime.UtcNow
        };
        AddRecord(record);
        GD.PrintErr($"[{service}] {ex.Message}");
        EmitSignal(SignalName.ErrorLogged, service, ex.Message);
    }

    public void LogError(string service, string message)
    {
        var record = new ErrorRecord
        {
            Service = service,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        AddRecord(record);
        GD.PrintErr($"[{service}] {message}");
        EmitSignal(SignalName.ErrorLogged, service, message);
    }

    public IReadOnlyList<ErrorRecord> GetRecentErrors() => _recentErrors;

    private void AddRecord(ErrorRecord record)
    {
        _recentErrors.Add(record);
        if (_recentErrors.Count > MaxErrorHistory)
            _recentErrors.RemoveAt(0);
    }

    public class ErrorRecord
    {
        public string Service { get; set; } = "";
        public string Message { get; set; } = "";
        public string StackTrace { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
