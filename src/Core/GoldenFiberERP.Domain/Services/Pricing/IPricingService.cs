using GoldenFiberERP.Domain.ValueObjects;

namespace GoldenFiberERP.Domain.Services.Pricing;

/// <summary>
/// Domain service for pricing calculations and business logic
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculate the selling price for a product based on customer and quantity
    /// </summary>
    /// <param name="basePrice">Base price of the product</param>
    /// <param name="customerId">Customer ID for customer-specific pricing</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <param name="currency">Currency for the calculation</param>
    /// <returns>Calculated price</returns>
    Money CalculatePrice(Money basePrice, int customerId, int quantity, string currency = "USD");

    /// <summary>
    /// Calculate discount amount for a customer
    /// </summary>
    /// <param name="subtotal">Subtotal amount</param>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Discount amount</returns>
    Money CalculateDiscount(Money subtotal, int customerId);

    /// <summary>
    /// Calculate quantity-based discount
    /// </summary>
    /// <param name="basePrice">Base price per unit</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <returns>Discounted unit price</returns>
    Money CalculateQuantityDiscount(Money basePrice, int quantity);

    /// <summary>
    /// Validate if a price is within acceptable business rules
    /// </summary>
    /// <param name="price">Price to validate</param>
    /// <param name="cost">Cost of the product</param>
    /// <returns>True if price is valid</returns>
    bool ValidatePrice(Money price, Money cost);

    /// <summary>
    /// Calculate markup percentage
    /// </summary>
    /// <param name="sellingPrice">Selling price</param>
    /// <param name="cost">Cost price</param>
    /// <returns>Markup percentage</returns>
    decimal CalculateMarkupPercentage(Money sellingPrice, Money cost);

    /// <summary>
    /// Calculate profit margin
    /// </summary>
    /// <param name="sellingPrice">Selling price</param>
    /// <param name="cost">Cost price</param>
    /// <returns>Profit margin as decimal (0.25 = 25%)</returns>
    decimal CalculateProfitMargin(Money sellingPrice, Money cost);
}
