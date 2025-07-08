using Microsoft.Extensions.Logging;
using MediatR;
using GoldenFiberERP.Domain.Events.Inventory;
using GoldenFiberERP.Application.Common.Interfaces;

namespace GoldenFiberERP.Application.Features.Inventory.EventHandlers;

/// <summary>
/// Handles ProductStockChangedEvent to manage stock-related notifications and business logic
/// </summary>
public class ProductStockChangedEventHandler : INotificationHandler<ProductStockChangedEvent>
{
    private readonly ILogger<ProductStockChangedEventHandler> _logger;
    private readonly IEmailService _emailService;

    public ProductStockChangedEventHandler(
        ILogger<ProductStockChangedEventHandler> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Handle(ProductStockChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ProductStockChangedEvent for Product ID: {ProductId}, " +
                              "Stock changed from {PreviousQuantity} to {NewQuantity}",
            notification.ProductId, notification.PreviousQuantity, notification.NewQuantity);

        try
        {
            // Handle low stock alerts
            if (notification.IsLowStock)
            {
                await HandleLowStockAlert(notification);
            }

            // Handle out of stock alerts
            if (notification.NewQuantity == 0)
            {
                await HandleOutOfStockAlert(notification);
            }

            // Handle large stock increases (potential restocking)
            if (notification.QuantityChanged > 100)
            {
                await HandleLargeStockIncrease(notification);
            }

            // Log audit trail
            LogStockChangeAudit(notification);

            _logger.LogInformation("Successfully processed ProductStockChangedEvent for Product ID: {ProductId}",
                notification.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductStockChangedEvent for Product ID: {ProductId}",
                notification.ProductId);
            throw;
        }
    }

    private async Task HandleLowStockAlert(ProductStockChangedEvent stockChanged)
    {
        var subject = "Low Stock Alert";
        var body = $@"
            ALERT: Product is running low on stock
            
            Product: {stockChanged.ProductName} (SKU: {stockChanged.ProductCode})
            Current Stock: {stockChanged.NewQuantity}
            Minimum Stock Level: {stockChanged.MinimumStockLevel}
            Previous Stock: {stockChanged.PreviousQuantity}
            Change Reason: {stockChanged.Reason}
            Changed At: {stockChanged.OccurredAt}
            
            Please consider reordering this product.
        ";

        try
        {
            await _emailService.SendAsync("inventory-manager@company.com", subject, body);
            
            _logger.LogInformation("Low stock alert sent for product: {ProductCode} (Current stock: {CurrentStock})",
                stockChanged.ProductCode, stockChanged.NewQuantity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send low stock alert for product: {ProductCode}",
                stockChanged.ProductCode);
        }
    }

    private async Task HandleOutOfStockAlert(ProductStockChangedEvent stockChanged)
    {
        var subject = "URGENT: Product Out of Stock";
        var body = $@"
            URGENT ALERT: Product is out of stock
            
            Product: {stockChanged.ProductName} (SKU: {stockChanged.ProductCode})
            Previous Stock: {stockChanged.PreviousQuantity}
            Change Reason: {stockChanged.Reason}
            Out of Stock Since: {stockChanged.OccurredAt}
            
            IMMEDIATE ACTION REQUIRED: This product needs to be restocked immediately.
        ";

        try
        {
            // Send to both inventory manager and procurement team
            await _emailService.SendAsync(new[] { "inventory-manager@company.com", "procurement@company.com" }, 
                subject, body);
            
            _logger.LogWarning("Out of stock alert sent for product: {ProductCode}",
                stockChanged.ProductCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send out of stock alert for product: {ProductCode}",
                stockChanged.ProductCode);
        }
    }

    private async Task HandleLargeStockIncrease(ProductStockChangedEvent stockChanged)
    {
        var subject = "Large Stock Increase Detected";
        var body = $@"
            INFO: Large stock increase detected
            
            Product: {stockChanged.ProductName} (SKU: {stockChanged.ProductCode})
            Stock Increase: +{stockChanged.QuantityChanged}
            Previous Stock: {stockChanged.PreviousQuantity}
            New Stock: {stockChanged.NewQuantity}
            Change Reason: {stockChanged.Reason}
            Changed At: {stockChanged.OccurredAt}
            
            This may indicate a restocking operation.
        ";

        try
        {
            await _emailService.SendAsync("inventory-manager@company.com", subject, body);
            
            _logger.LogInformation("Large stock increase notification sent for product: {ProductCode} (+{Increase})",
                stockChanged.ProductCode, stockChanged.QuantityChanged);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send large stock increase notification for product: {ProductCode}",
                stockChanged.ProductCode);
        }
    }

    private void LogStockChangeAudit(ProductStockChangedEvent stockChanged)
    {
        _logger.LogInformation("AUDIT: Stock changed - Product: {ProductCode}, " +
                              "Previous: {PreviousQuantity}, New: {NewQuantity}, " +
                              "Change: {QuantityChanged}, Reason: {Reason}, " +
                              "IsLowStock: {IsLowStock}, ChangedAt: {ChangedAt}",
            stockChanged.ProductCode,
            stockChanged.PreviousQuantity,
            stockChanged.NewQuantity,
            stockChanged.QuantityChanged,
            stockChanged.Reason,
            stockChanged.IsLowStock,
            stockChanged.OccurredAt);
    }
}
