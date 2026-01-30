using Vacuum.Data.Enums;
using Vacuum.Data.Models;

namespace Vacuum.Services.Market;

/// <summary>
/// Validates market orders for funds, quantity, and access control.
/// </summary>
public static class OrderValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public static ValidationResult Ok() => new() { IsValid = true };
        public static ValidationResult Fail(string error) => new() { IsValid = false, Error = error };
    }

    public static ValidationResult ValidateOrder(MarketOrderData order)
    {
        if (string.IsNullOrEmpty(order.ItemTypeId))
            return ValidationResult.Fail("Item type is required.");
        if (order.Quantity <= 0)
            return ValidationResult.Fail("Quantity must be positive.");
        if (order.Price <= 0)
            return ValidationResult.Fail("Price must be positive.");
        if (string.IsNullOrEmpty(order.CharacterId))
            return ValidationResult.Fail("Character ID is required.");
        return ValidationResult.Ok();
    }

    public static ValidationResult ValidateFunds(double balance, MarketOrderData order)
    {
        if (order.IsBuyOrder)
        {
            double totalCost = order.Quantity * order.Price;
            if (balance < totalCost)
                return ValidationResult.Fail($"Insufficient funds. Need {totalCost:F2}, have {balance:F2}.");
        }
        return ValidationResult.Ok();
    }

    public static ValidationResult ValidateAccess(MarketAccessLevel accessLevel)
    {
        if (accessLevel == MarketAccessLevel.Denied)
            return ValidationResult.Fail("Access denied by faction restrictions.");
        return ValidationResult.Ok();
    }

    public static ValidationResult ValidateQuantity(int available, int requested)
    {
        if (available < requested)
            return ValidationResult.Fail($"Insufficient quantity. Available: {available}, requested: {requested}.");
        return ValidationResult.Ok();
    }
}
