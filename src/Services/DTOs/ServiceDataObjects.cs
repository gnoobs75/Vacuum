using System.Collections.Generic;

namespace Vacuum.Services.DTOs;

/// <summary>
/// Common request/response DTOs for service interactions.
/// </summary>
public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();

    public static ServiceResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ServiceResponse<T> Fail(string error) => new() { Success = false, ErrorMessage = error };
}

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
