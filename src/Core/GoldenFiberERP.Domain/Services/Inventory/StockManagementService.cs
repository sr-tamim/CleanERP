using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Domain.Events.Inventory;
using GoldenFiberERP.Domain.Exceptions;
using GoldenFiberERP.Domain.Services.Inventory;

namespace GoldenFiberERP.Domain.Services.Inventory;

/// <summary>
/// Implementation of stock management domain service
/// </summary>
public class StockManagementService : IStockManagementService
{
    public async Task<bool> ReserveStockAsync(Product product, int quantity, string reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (!HasSufficientStock(product, quantity))
        {
            return false;
        }

        product.ReserveStock(quantity);
        return await Task.FromResult(true);
    }

    public async Task ReleaseStockAsync(Product product, int quantity, string reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        product.ReleaseStock(quantity);
        await Task.CompletedTask;
    }

    public async Task AdjustStockAsync(Product product, int quantity, string reason)
    {
        if (quantity == 0)
            return;

        if (quantity < 0 && !HasSufficientStock(product, Math.Abs(quantity)))
        {
            throw new InsufficientStockException(
                $"Cannot adjust stock for product {product.Name}. Insufficient stock. Available: {product.StockQuantity}, Required: {Math.Abs(quantity)}");
        }

        product.AdjustStock(quantity, reason);
        await Task.CompletedTask;
    }

    public bool HasSufficientStock(Product product, int requiredQuantity)
    {
        return product.StockQuantity >= requiredQuantity;
    }

    public bool IsLowStock(Product product)
    {
        return product.StockQuantity <= product.MinimumStockLevel;
    }

    public async Task TransferStockAsync(Product fromProduct, Product toProduct, int quantity, string reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (!HasSufficientStock(fromProduct, quantity))
        {
            throw new InsufficientStockException(
                $"Cannot transfer stock from product {fromProduct.Name}. Insufficient stock. Available: {fromProduct.StockQuantity}, Required: {quantity}");
        }

        // Decrease stock from source product
        await AdjustStockAsync(fromProduct, -quantity, $"Transfer to {toProduct.Name}: {reason}");

        // Increase stock in destination product
        await AdjustStockAsync(toProduct, quantity, $"Transfer from {fromProduct.Name}: {reason}");
    }
}
