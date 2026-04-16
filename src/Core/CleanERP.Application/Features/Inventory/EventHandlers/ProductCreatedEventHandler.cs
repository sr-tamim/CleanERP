using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Domain.Events.Inventory;
using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Application.Features.Inventory.EventHandlers;

/// <summary>
/// Handles ProductCreatedEvent to perform post-creation tasks
/// </summary>
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;
    private readonly IEmailService _emailService;

    public ProductCreatedEventHandler(
        ILogger<ProductCreatedEventHandler> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ProductCreatedEvent for Product ID: {ProductId}, SKU: {ProductCode}",
            notification.ProductId, notification.ProductCode);

        try
        {
            // Send notification to inventory management team
            await NotifyInventoryTeam(notification);

            // Log audit trail
            LogAuditTrail(notification);

            // Additional business logic can be added here
            // - Update search indexes
            // - Sync with external systems
            // - Generate product barcodes
            // - Set up automated reorder rules

            _logger.LogInformation("Successfully processed ProductCreatedEvent for Product ID: {ProductId}",
                notification.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductCreatedEvent for Product ID: {ProductId}",
                notification.ProductId);
            throw;
        }
    }

    private async Task NotifyInventoryTeam(ProductCreatedEvent productCreated)
    {
        var subject = "New Product Created";
        var body = $@"
            A new product has been created in the system:
            
            Product ID: {productCreated.ProductId}
            SKU: {productCreated.ProductCode}
            Name: {productCreated.ProductName}
            Category: {productCreated.Category}
            Price: ${productCreated.Price:F2}
            Initial Stock: {productCreated.InitialStockQuantity}
            Created By: User ID {productCreated.CreatedBy}
            Created At: {productCreated.OccurredAt}
        ";

        try
        {
            // In a real implementation, you would send to actual inventory team email addresses
            await _emailService.SendAsync("inventory-team@company.com", subject, body);
            
            _logger.LogInformation("Notification sent to inventory team for new product: {ProductCode}",
                productCreated.ProductCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email notification for new product: {ProductCode}",
                productCreated.ProductCode);
            // Don't throw - this is not critical enough to fail the entire operation
        }
    }

    private void LogAuditTrail(ProductCreatedEvent productCreated)
    {
        _logger.LogInformation("AUDIT: Product created - ID: {ProductId}, SKU: {ProductCode}, " +
                              "Name: {ProductName}, Category: {Category}, Price: {Price}, " +
                              "InitialStock: {InitialStock}, CreatedBy: {CreatedBy}, " +
                              "CreatedAt: {CreatedAt}",
            productCreated.ProductId,
            productCreated.ProductCode,
            productCreated.ProductName,
            productCreated.Category,
            productCreated.Price,
            productCreated.InitialStockQuantity,
            productCreated.CreatedBy,
            productCreated.OccurredAt);
    }
}
