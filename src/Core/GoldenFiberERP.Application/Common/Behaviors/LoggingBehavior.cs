using MediatR;
using Microsoft.Extensions.Logging;

namespace GoldenFiberERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for logging requests and responses
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogInformation("Handling request {RequestName} with ID {RequestId}: {@Request}",
            requestName, requestId, request);

        try
        {
            var response = await next();

            _logger.LogInformation("Request {RequestName} with ID {RequestId} completed successfully",
                requestName, requestId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed",
                requestName, requestId);
            throw;
        }
    }
}
