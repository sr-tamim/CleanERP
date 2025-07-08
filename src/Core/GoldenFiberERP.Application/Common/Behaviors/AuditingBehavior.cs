using Microsoft.Extensions.Logging;
using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;

namespace GoldenFiberERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for auditing user actions
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<AuditingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public AuditingBehavior(
        ILogger<AuditingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";
        var timestamp = _dateTime.UtcNow;

        // Log the action
        _logger.LogInformation("Audit: User {UserId} is executing {RequestName} at {Timestamp}",
            userId, requestName, timestamp);

        try
        {
            var response = await next();

            // Log successful completion
            _logger.LogInformation("Audit: User {UserId} successfully completed {RequestName} at {Timestamp}",
                userId, requestName, _dateTime.UtcNow);

            return response;
        }
        catch (Exception ex)
        {
            // Log failed execution
            _logger.LogError(ex, "Audit: User {UserId} failed to execute {RequestName} at {Timestamp}. Error: {Error}",
                userId, requestName, _dateTime.UtcNow, ex.Message);

            throw;
        }
    }
}
