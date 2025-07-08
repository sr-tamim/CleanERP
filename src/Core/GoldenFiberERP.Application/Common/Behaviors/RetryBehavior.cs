using Microsoft.Extensions.Logging;
using MediatR;

namespace GoldenFiberERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for handling retries on transient failures
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private const int MaxRetryAttempts = 3;
    private const int DelayMilliseconds = 100;

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts && IsTransientFailure(ex))
            {
                _logger.LogWarning("Request {RequestName} failed on attempt {Attempt}/{MaxAttempts}. Error: {Error}. Retrying...",
                    requestName, attempt, MaxRetryAttempts, ex.Message);

                await Task.Delay(DelayMilliseconds * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {RequestName} failed permanently on attempt {Attempt}/{MaxAttempts}",
                    requestName, attempt, MaxRetryAttempts);
                throw;
            }
        }

        // This should never be reached due to the exception handling above
        throw new InvalidOperationException("Retry logic failed unexpectedly");
    }

    private static bool IsTransientFailure(Exception exception)
    {
        // Define which exceptions are considered transient and worth retrying
        return exception switch
        {
            System.Net.Sockets.SocketException => true,
            System.Net.Http.HttpRequestException => true,
            TimeoutException => true,
            System.Data.Common.DbException dbEx => IsTransientDatabaseException(dbEx),
            _ => false
        };
    }

    private static bool IsTransientDatabaseException(System.Data.Common.DbException dbException)
    {
        // Common transient database error codes
        var transientErrorNumbers = new[]
        {
            -2,     // Timeout
            2,      // Connection timeout
            53,     // Network path not found
            121,    // Semaphore timeout
            1205,   // Deadlock
            1222,   // Lock request timeout
            8645,   // Timeout waiting for memory resource
            8651    // Low memory condition
        };

        return dbException.Data.Contains("Number") && 
               transientErrorNumbers.Contains((int)dbException.Data["Number"]!);
    }
}
