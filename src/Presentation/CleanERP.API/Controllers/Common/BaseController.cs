using Microsoft.AspNetCore.Mvc;
using CleanERP.API.Models;
using CleanERP.Application.Common.Models;

namespace CleanERP.API.Controllers.Common;

/// <summary>
/// Base controller with common functionality
/// Eliminates the need for try-catch in every action method
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Handles Result&lt;T&gt; responses and converts them to consistent API responses
    /// </summary>
    protected ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result, string? successMessage = null)
    {
        if (result.Succeeded)
        {
            return Ok(ApiResponse<T>.SuccessResult(result.Data!, successMessage));
        }

        return BadRequest(ApiResponse<T>.ErrorResult(result.Errors, "Operation failed"));
    }

    /// <summary>
    /// Handles Result responses without data
    /// </summary>
    protected ActionResult<ApiResponse> HandleResult(Result result, string? successMessage = null)
    {
        if (result.Succeeded)
        {
            return Ok(ApiResponse.SuccessResult(successMessage));
        }

        return BadRequest(ApiResponse.ErrorResult(result.Errors, "Operation failed"));
    }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    protected ActionResult<ApiResponse> Success(string? message = null)
    {
        return Ok(ApiResponse.SuccessResult(message));
    }

    /// <summary>
    /// Creates a created response (201) with data
    /// </summary>
    protected ActionResult<ApiResponse<T>> Created<T>(T data, string? message = null)
    {
        return StatusCode(201, ApiResponse<T>.SuccessResult(data, message));
    }

    /// <summary>
    /// Creates a created response (201) without data
    /// </summary>
    protected ActionResult<ApiResponse> Created(string? message = null)
    {
        return StatusCode(201, ApiResponse.SuccessResult(message));
    }

    /// <summary>
    /// Creates a bad request response
    /// </summary>
    protected ActionResult<ApiResponse> BadRequest(string message, List<string>? errors = null)
    {
        return BadRequest(ApiResponse.ErrorResult(errors ?? new List<string>(), message));
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    protected ActionResult<ApiResponse> NotFound(string message = "Resource not found")
    {
        return NotFound(ApiResponse.ErrorResult(message));
    }

    /// <summary>
    /// Gets the current request ID from headers
    /// </summary>
    protected string GetRequestId()
    {
        return HttpContext.Response.Headers["X-Request-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    }
}
