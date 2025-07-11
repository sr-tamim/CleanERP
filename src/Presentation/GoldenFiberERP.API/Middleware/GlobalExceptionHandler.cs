using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Shared.Exceptions;
using GoldenFiberERP.API.Models;
using System.Net;
using System.Text.Json;

namespace GoldenFiberERP.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var requestId = httpContext.Response.Headers["X-Request-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        
        // Log the exception with request context
        _logger.LogError(exception, 
            "[{RequestId}] Exception occurred in {Method} {Path}: {ExceptionType} - {Message}",
            requestId,
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.GetType().Name,
            exception.Message);

        var response = CreateErrorResponse(exception, requestId);
        
        httpContext.Response.StatusCode = response.StatusCode;
        httpContext.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response.ApiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

        return true;
    }

    private (int StatusCode, ApiResponse ApiResponse) CreateErrorResponse(Exception exception, string requestId)
    {
        return exception switch
        {
            ValidationException validationEx => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Validation failed",
                    errors: validationEx.Errors.SelectMany(e => e.Value).ToList(),
                    requestId: requestId
                )
            ),
            
            NotFoundException notFoundEx => (
                StatusCode: (int)HttpStatusCode.NotFound,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Resource not found",
                    errors: new List<string> { notFoundEx.Message },
                    requestId: requestId
                )
            ),
            
            BusinessException businessEx => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Business rule violation",
                    errors: new List<string> { businessEx.Message },
                    requestId: requestId,
                    data: new { ErrorCode = businessEx.ErrorCode }
                )
            ),
            
            BusinessLogicException businessLogicEx => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Business logic violation",
                    errors: new List<string> { businessLogicEx.Message },
                    requestId: requestId,
                    data: new { ErrorCode = businessLogicEx.ErrorCode }
                )
            ),
            
            ForbiddenException forbiddenEx => (
                StatusCode: (int)HttpStatusCode.Forbidden,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Operation forbidden",
                    errors: new List<string> { forbiddenEx.Message },
                    requestId: requestId
                )
            ),
            
            ConflictException conflictEx => (
                StatusCode: (int)HttpStatusCode.Conflict,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Resource conflict",
                    errors: new List<string> { conflictEx.Message },
                    requestId: requestId
                )
            ),
            
            SecurityException securityEx => (
                StatusCode: (int)HttpStatusCode.Forbidden,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Access denied",
                    errors: new List<string> { securityEx.Message },
                    requestId: requestId,
                    data: new { UserId = securityEx.UserId, Action = securityEx.Action }
                )
            ),
            
            ConcurrencyException concurrencyEx => (
                StatusCode: (int)HttpStatusCode.Conflict,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Concurrency conflict",
                    errors: new List<string> { concurrencyEx.Message },
                    requestId: requestId,
                    data: new { EntityName = concurrencyEx.EntityName, EntityId = concurrencyEx.EntityId }
                )
            ),
            
            ExternalServiceException externalEx => (
                StatusCode: (int)HttpStatusCode.BadGateway,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "External service error",
                    errors: new List<string> { externalEx.Message },
                    requestId: requestId,
                    data: new { ServiceName = externalEx.ServiceName }
                )
            ),
            
            UnauthorizedAccessException unauthorizedEx => (
                StatusCode: (int)HttpStatusCode.Unauthorized,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Authentication required",
                    errors: new List<string> { unauthorizedEx.Message },
                    requestId: requestId
                )
            ),
            
            ArgumentException argumentEx => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "Invalid argument",
                    errors: new List<string> { argumentEx.Message },
                    requestId: requestId
                )
            ),
            
            _ => (
                StatusCode: (int)HttpStatusCode.InternalServerError,
                ApiResponse: CreateApiResponse(
                    success: false,
                    message: "An internal server error occurred",
                    errors: new List<string> { "Please try again later or contact support if the problem persists" },
                    requestId: requestId
                )
            )
        };
    }

    private ApiResponse CreateApiResponse(
        bool success, 
        string message, 
        List<string> errors, 
        string requestId, 
        object? data = null)
    {
        return new ApiResponse
        {
            Success = success,
            Message = message,
            Errors = errors,
            RequestId = requestId,
            Data = data
        };
    }
}
