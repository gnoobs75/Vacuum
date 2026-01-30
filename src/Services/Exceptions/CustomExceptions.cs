using System;

namespace Vacuum.Services;

/// <summary>General service operation failure.</summary>
public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Data validation failure.</summary>
public class ValidationException : Exception
{
    public string? FieldName { get; }
    public ValidationException(string message, string? fieldName = null) : base(message)
    {
        FieldName = fieldName;
    }
}

/// <summary>Requested entity not found.</summary>
public class NotFoundException : Exception
{
    public string? EntityType { get; }
    public string? EntityId { get; }
    public NotFoundException(string entityType, string entityId)
        : base($"{entityType} '{entityId}' not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
