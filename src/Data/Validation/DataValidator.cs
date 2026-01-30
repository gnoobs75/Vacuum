using System;
using System.Collections.Generic;

namespace Vacuum.Data.Validation;

/// <summary>
/// Data validation utilities for ensuring model integrity.
/// </summary>
public static class DataValidator
{
    public static ValidationResult Validate(params (bool condition, string error)[] rules)
    {
        var errors = new List<string>();
        foreach (var (condition, error) in rules)
        {
            if (!condition)
                errors.Add(error);
        }
        return new ValidationResult(errors);
    }

    public static bool IsValidId(string? id) => !string.IsNullOrWhiteSpace(id);

    public static bool IsPositive(float value) => value > 0;

    public static bool IsInRange(float value, float min, float max) => value >= min && value <= max;
}

public class ValidationResult
{
    public List<string> Errors { get; }
    public bool IsValid => Errors.Count == 0;

    public ValidationResult(List<string> errors)
    {
        Errors = errors;
    }
}
