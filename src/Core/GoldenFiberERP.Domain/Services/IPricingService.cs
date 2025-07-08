using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Domain.Services;

/// <summary>
/// Domain service for pricing calculations and business logic
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculate the final price for a product considering customer-specific discounts
    /// </summary>
    /// <param name="product">Product to calculate price for</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <param name="customerId">Customer ID for discount calculations</param>
    /// <returns>Final calculated price</returns>
    decimal CalculatePrice(Product product, int quantity, int? customerId = null);

    /// <summary>
    /// Calculate discount percentage based on quantity and customer tier
    /// </summary>
    /// <param name="baseAmount">Base amount before discount</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <param name="customerId">Customer ID for tier-based discounts</param>
    /// <returns>Discount percentage (0-100)</returns>
    decimal CalculateDiscountPercentage(decimal baseAmount, int quantity, int? customerId = null);

    /// <summary>
    /// Calculate bulk discount for large quantity purchases
    /// </summary>
    /// <param name="product">Product being purchased</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <returns>Bulk discount percentage</returns>
    decimal CalculateBulkDiscount(Product product, int quantity);

    /// <summary>
    /// Validate if a price is within acceptable business rules
    /// </summary>
    /// <param name="product">Product being priced</param>
    /// <param name="proposedPrice">Proposed selling price</param>
    /// <returns>True if price is valid according to business rules</returns>
    bool ValidatePrice(Product product, decimal proposedPrice);

    /// <summary>
    /// Calculate profit margin for a given selling price
    /// </summary>
    /// <param name="product">Product to calculate margin for</param>
    /// <param name="sellingPrice">Proposed selling price</param>
    /// <returns>Profit margin percentage</returns>
    decimal CalculateProfitMargin(Product product, decimal sellingPrice);

    /// <summary>
    /// Get minimum allowed selling price (cost + minimum margin)
    /// </summary>
    /// <param name="product">Product to calculate minimum price for</param>
    /// <returns>Minimum allowed selling price</returns>
    decimal GetMinimumSellingPrice(Product product);
}
