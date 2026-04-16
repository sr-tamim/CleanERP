using CleanERP.Domain.Entities.Inventory;

namespace CleanERP.Domain.Services.Inventory;

/// <summary>
/// Domain service for stock management business logic
/// </summary>
public interface IStockManagementService
{
    /// <summary>
    /// Reserve stock for a product
    /// </summary>
    /// <param name="product">Product to reserve stock for</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <param name="reason">Reason for reservation</param>
    /// <returns>True if reservation was successful</returns>
    Task<bool> ReserveStockAsync(Product product, int quantity, string reason);

    /// <summary>
    /// Release reserved stock for a product
    /// </summary>
    /// <param name="product">Product to release stock for</param>
    /// <param name="quantity">Quantity to release</param>
    /// <param name="reason">Reason for release</param>
    Task ReleaseStockAsync(Product product, int quantity, string reason);

    /// <summary>
    /// Adjust stock quantity for a product
    /// </summary>
    /// <param name="product">Product to adjust stock for</param>
    /// <param name="quantity">Quantity to adjust (positive for increase, negative for decrease)</param>
    /// <param name="reason">Reason for adjustment</param>
    Task AdjustStockAsync(Product product, int quantity, string reason);

    /// <summary>
    /// Check if a product has sufficient stock
    /// </summary>
    /// <param name="product">Product to check</param>
    /// <param name="requiredQuantity">Required quantity</param>
    /// <returns>True if sufficient stock is available</returns>
    bool HasSufficientStock(Product product, int requiredQuantity);

    /// <summary>
    /// Check if a product is low on stock
    /// </summary>
    /// <param name="product">Product to check</param>
    /// <returns>True if product is low on stock</returns>
    bool IsLowStock(Product product);

    /// <summary>
    /// Transfer stock between products (for variants or substitutes)
    /// </summary>
    /// <param name="fromProduct">Source product</param>
    /// <param name="toProduct">Destination product</param>
    /// <param name="quantity">Quantity to transfer</param>
    /// <param name="reason">Reason for transfer</param>
    Task TransferStockAsync(Product fromProduct, Product toProduct, int quantity, string reason);
}
