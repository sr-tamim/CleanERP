using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Text.Json;

namespace CleanERP.API.Filters;

/// <summary>
/// Action filter for comprehensive API request/response logging
/// Similar to NestJS interceptors
/// </summary>
public class ApiLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Log request details
        var request = context.HttpContext.Request;
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;

        _logger.LogInformation(
            "[{RequestId}] {Method} {Path} - Controller: {Controller}, Action: {Action} - Request started",
            requestId,
            request.Method,
            request.Path,
            controllerName,
            actionName);

        // Log request parameters (excluding sensitive data)
        if (context.ActionArguments.Any())
        {
            var sanitizedArgs = SanitizeArguments(context.ActionArguments);
            _logger.LogInformation(
                "[{RequestId}] Request parameters: {@Parameters}",
                requestId,
                sanitizedArgs);
        }

        // Add request ID to response headers
        context.HttpContext.Response.Headers.Append("X-Request-ID", requestId);

        // Execute the action
        var executedContext = await next();

        stopwatch.Stop();

        // Log response details
        var response = context.HttpContext.Response;
        var statusCode = response.StatusCode;
        var duration = stopwatch.ElapsedMilliseconds;

        if (executedContext.Exception == null)
        {
            _logger.LogInformation(
                "[{RequestId}] {Method} {Path} - Response: {StatusCode} - Duration: {Duration}ms - Completed successfully",
                requestId,
                request.Method,
                request.Path,
                statusCode,
                duration);
        }
        else
        {
            _logger.LogError(executedContext.Exception,
                "[{RequestId}] {Method} {Path} - Exception occurred - Duration: {Duration}ms",
                requestId,
                request.Method,
                request.Path,
                duration);
        }

        // Log performance warning for slow requests
        if (duration > 3000) // 3 seconds
        {
            _logger.LogWarning(
                "[{RequestId}] Slow request detected: {Method} {Path} took {Duration}ms",
                requestId,
                request.Method,
                request.Path,
                duration);
        }
    }

    private object SanitizeArguments(IDictionary<string, object?> arguments)
    {
        var sanitized = new Dictionary<string, object?>();

        foreach (var arg in arguments)
        {
            if (IsSensitiveField(arg.Key))
            {
                sanitized[arg.Key] = "***REDACTED***";
            }
            else if (arg.Value != null)
            {
                // Limit large objects to prevent log overflow
                var serialized = JsonSerializer.Serialize(arg.Value);
                if (serialized.Length > 1000)
                {
                    sanitized[arg.Key] = $"[LARGE_OBJECT: {serialized.Length} chars]";
                }
                else
                {
                    sanitized[arg.Key] = arg.Value;
                }
            }
            else
            {
                sanitized[arg.Key] = null;
            }
        }

        return sanitized;
    }

    private static bool IsSensitiveField(string fieldName)
    {
        var sensitiveFields = new[] { "password", "token", "secret", "key", "auth" };
        return sensitiveFields.Any(field => fieldName.ToLower().Contains(field));
    }
}
