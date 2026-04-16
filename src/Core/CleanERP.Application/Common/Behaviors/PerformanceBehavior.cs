using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CleanERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for logging request performance
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // Log if request took longer than 500ms
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName, elapsedMilliseconds, request);
        }
        else
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("Request: {Name} completed in {ElapsedMilliseconds}ms",
                requestName, elapsedMilliseconds);
        }

        return response;
    }
}
