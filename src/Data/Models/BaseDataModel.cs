using System;

namespace Vacuum.Data.Models;

/// <summary>
/// Base class for all persistable data models. Provides common fields.
/// </summary>
public abstract class BaseDataModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
