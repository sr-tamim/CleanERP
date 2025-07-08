using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Domain.Services;

/// <summary>
/// Domain service for stock management business logic
/// </summary>
public interface IStockManagementService
{
    /// <summary>
    /// Reserve stock for a product if available
    /// </summary>
    /// <param name="product">Product to reserve stock for</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <param name="reason">Reason for reservation</param>
    /// <param name="reservedBy">User making the reservation</param>
    /// <returns>True if stock was successfully reserved</returns>
    Task<bool> ReserveStockAsync(Product product, int quantity, string reason, int? reservedBy = null);

    /// <summary>
    /// Release previously reserved stock
    /// </summary>
    /// <param name="product">Product to release stock for</param>
    /// <param name="quantity">Quantity to release</param>
    /// <param name="reason">Reason for release</param>
    /// <param name="releasedBy">User releasing the stock</param>
    Task ReleaseStockAsync(Product product, int quantity, string reason, int? releasedBy = null);

    /// <summary>
    /// Adjust stock quantity with business rule validation
    /// </summary>
    /// <param name="product">Product to adjust stock for</param>
    /// <param name="quantityChange">Change in quantity (positive for increase, negative for decrease)</param>
    /// <param name="reason">Reason for adjustment</param>
    /// <param name="adjustedBy">User making the adjustment</param>
    Task AdjustStockAsync(Product product, int quantityChange, string reason, int? adjustedBy = null);

    /// <summary>
    /// Check if stock adjustment would result in low stock condition
    /// </summary>
    /// <param name="product">Product to check</param>
    /// <param name="quantityChange">Proposed quantity change</param>
    /// <returns>True if the adjustment would result in low stock</returns>
    bool WouldResultInLowStock(Product product, int quantityChange);

    /// <summary>
    /// Validate stock operation before execution
    /// </summary>
    /// <param name="product">Product for the operation</param>
    /// <param name="quantityChange">Proposed quantity change</param>
    /// <param name="operationType">Type of operation (Reserve, Release, Adjust)</param>
    /// <returns>Validation result with any errors</returns>
    (bool IsValid, string[] Errors) ValidateStockOperation(Product product, int quantityChange, string operationType);
}
