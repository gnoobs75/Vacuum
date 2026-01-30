using System;

namespace Vacuum.Tasks;

/// <summary>
/// Encapsulates the result of a background task execution.
/// </summary>
public class TaskResult
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }

    public static TaskResult Ok(object? data = null) => new() { Success = true, Data = data };
    public static TaskResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
