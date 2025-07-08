using Microsoft.Extensions.Logging;
using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;

namespace GoldenFiberERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for sending notifications based on request outcomes
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class NotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<NotificationBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public NotificationBehavior(
        ILogger<NotificationBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;

        try
        {
            var response = await next();

            // Send success notifications for important operations
            if (ShouldNotifyOnSuccess(requestName))
            {
                await SendSuccessNotification(requestName, userId);
            }

            return response;
        }
        catch (Exception ex)
        {
            // Send failure notifications for critical operations
            if (ShouldNotifyOnFailure(requestName))
            {
                await SendFailureNotification(requestName, userId, ex.Message);
            }

            throw;
        }
    }

    private static bool ShouldNotifyOnSuccess(string requestName)
    {
        // Define which operations should trigger success notifications
        var criticalOperations = new[]
        {
            "CreateProductCommand",
            "UpdateProductCommand",
            "DeleteProductCommand",
            "CreateOrderCommand",
            "ProcessPaymentCommand",
            "CreateUserCommand"
        };

        return criticalOperations.Contains(requestName);
    }

    private static bool ShouldNotifyOnFailure(string requestName)
    {
        // Define which operations should trigger failure notifications
        var importantOperations = new[]
        {
            "CreateProductCommand",
            "UpdateProductCommand",
            "DeleteProductCommand",
            "CreateOrderCommand",
            "ProcessPaymentCommand",
            "CreateUserCommand",
            "ImportDataCommand",
            "ExportDataCommand"
        };

        return importantOperations.Contains(requestName);
    }

    private async Task SendSuccessNotification(string operation, string? userId)
    {
        _logger.LogInformation("Success notification: Operation {Operation} completed successfully by user {UserId}",
            operation, userId ?? "Anonymous");

        // Here you would integrate with your notification service
        // Example: await _notificationService.SendAsync(userId, "Operation Completed", $"{operation} was successful");
        
        await Task.CompletedTask;
    }

    private async Task SendFailureNotification(string operation, string? userId, string errorMessage)
    {
        _logger.LogWarning("Failure notification: Operation {Operation} failed for user {UserId}. Error: {Error}",
            operation, userId ?? "Anonymous", errorMessage);

        // Here you would integrate with your notification service
        // Example: await _notificationService.SendAsync(userId, "Operation Failed", $"{operation} failed: {errorMessage}");
        
        await Task.CompletedTask;
    }
}
