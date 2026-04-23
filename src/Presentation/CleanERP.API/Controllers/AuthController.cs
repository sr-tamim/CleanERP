using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MediatR;
using GoldenFiberERP.Application.Features.Authentication.Commands.Login;
using GoldenFiberERP.Application.Features.Authentication.Commands.RefreshToken;
using GoldenFiberERP.Application.Features.Authentication.Commands.Logout;
using GoldenFiberERP.Application.Features.Authentication.Commands.Register;
using GoldenFiberERP.Application.Features.Authentication.Queries.GetCurrentUser;
using GoldenFiberERP.Application.Features.Authentication.Queries.ValidateToken;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.API.Configuration;
using GoldenFiberERP.API.Attributes;
using GoldenFiberERP.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace CleanERP.API.Controllers;

/// <summary>
/// Authentication and authorization endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CookieSettings _cookieSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IOptions<CookieSettings> cookieSettings,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _cookieSettings = cookieSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>User information (tokens set as secure cookies)</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand
            {
                UsernameOrEmail = request.UsernameOrEmail,
                Password = request.Password,
                IpAddress = GetClientIpAddress(),
                RememberMe = request.RememberMe
            };

            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed for user {UsernameOrEmail}: {Error}", 
                    request.UsernameOrEmail, result.Error);
                return Unauthorized(new { Message = result.Error });
            }

            // Set secure HTTP-only cookies
            SetAuthenticationCookies(result.Value.AccessToken, result.Value.RefreshToken, request.RememberMe);

            var response = new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                User = result.Value.User
            };

            _logger.LogInformation("User {UserId} logged in successfully", result.Value.User.Id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {UsernameOrEmail}", request.UsernameOrEmail);
            return StatusCode(500, new { Message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refresh access token endpoint
    /// </summary>
    /// <returns>New tokens set as secure cookies</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { Message = "Refresh token not found" });
            }

            var command = new RefreshTokenCommand
            {
                RefreshToken = refreshToken,
                IpAddress = GetClientIpAddress()
            };

            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                ClearAuthenticationCookies();
                return Unauthorized(new { Message = result.Error });
            }

            // Set new secure HTTP-only cookies
            SetAuthenticationCookies(result.Value.AccessToken, result.Value.RefreshToken, false);

            var response = new RefreshResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                User = result.Value.User
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            ClearAuthenticationCookies();
            return StatusCode(500, new { Message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// User logout endpoint
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), 200)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var command = new LogoutCommand
                {
                    RefreshToken = refreshToken,
                    IpAddress = GetClientIpAddress()
                };

                await _mediator.Send(command);
            }

            ClearAuthenticationCookies();

            var response = new LogoutResponse
            {
                Success = true,
                Message = "Logout successful"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            ClearAuthenticationCookies();
            return Ok(new LogoutResponse { Success = true, Message = "Logout completed" });
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var query = new GetCurrentUserQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { Message = result.Error });
            }

            var response = new CurrentUserResponse
            {
                Success = true,
                User = result.Value
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return StatusCode(500, new { Message = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// User registration endpoint
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword
            };

            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.Error });
            }

            var response = new RegisterResponse
            {
                Success = true,
                Message = "Registration successful",
                UserId = result.Value
            };

            _logger.LogInformation("User registered successfully with ID: {UserId}", result.Value);
            return CreatedAtAction(nameof(GetCurrentUser), new { id = result.Value }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {UserName}", request.UserName);
            return StatusCode(500, new { Message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Validate token endpoint
    /// </summary>
    /// <param name="request">Token validation request</param>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateTokenResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var query = new ValidateTokenQuery { Token = request.Token };
            var result = await _mediator.Send(query);

            var response = new ValidateTokenResponse
            {
                IsValid = result.IsSuccess,
                Message = result.IsSuccess ? "Token is valid" : result.Error
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Ok(new ValidateTokenResponse { IsValid = false, Message = "Token validation failed" });
        }
    }

    #region Private Methods

    private void SetAuthenticationCookies(string accessToken, string refreshToken, bool rememberMe)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = _cookieSettings.HttpOnly,
            Secure = _cookieSettings.Secure,
            SameSite = Enum.Parse<SameSiteMode>(_cookieSettings.SameSite, true),
            Path = _cookieSettings.Path,
            Expires = DateTime.UtcNow.AddMinutes(_cookieSettings.AccessTokenExpiryMinutes)
        };

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = _cookieSettings.HttpOnly,
            Secure = _cookieSettings.Secure,
            SameSite = Enum.Parse<SameSiteMode>(_cookieSettings.SameSite, true),
            Path = _cookieSettings.Path,
            Expires = rememberMe 
                ? DateTime.UtcNow.AddDays(_cookieSettings.RefreshTokenExpiryDays)
                : DateTime.UtcNow.AddDays(1) // Session cookie for 1 day if not remember me
        };

        if (!string.IsNullOrEmpty(_cookieSettings.Domain))
        {
            accessTokenOptions.Domain = _cookieSettings.Domain;
            refreshTokenOptions.Domain = _cookieSettings.Domain;
        }

        Response.Cookies.Append(_cookieSettings.AccessTokenCookieName, accessToken, accessTokenOptions);
        Response.Cookies.Append(_cookieSettings.RefreshTokenCookieName, refreshToken, refreshTokenOptions);
    }

    private void ClearAuthenticationCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = _cookieSettings.HttpOnly,
            Secure = _cookieSettings.Secure,
            SameSite = Enum.Parse<SameSiteMode>(_cookieSettings.SameSite, true),
            Path = _cookieSettings.Path,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        if (!string.IsNullOrEmpty(_cookieSettings.Domain))
        {
            cookieOptions.Domain = _cookieSettings.Domain;
        }

        Response.Cookies.Append(_cookieSettings.AccessTokenCookieName, "", cookieOptions);
        Response.Cookies.Append(_cookieSettings.RefreshTokenCookieName, "", cookieOptions);
    }

    private string? GetRefreshTokenFromCookie()
    {
        return Request.Cookies[_cookieSettings.RefreshTokenCookieName];
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    #endregion
}

#region Request/Response Models

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Register request model
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Validate token request model
/// </summary>
public class ValidateTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// Refresh response model
/// </summary>
public class RefreshResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// Logout response model
/// </summary>
public class LogoutResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Current user response model
/// </summary>
public class CurrentUserResponse
{
    public bool Success { get; set; }
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// Register response model
/// </summary>
public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
}

/// <summary>
/// Validate token response model
/// </summary>
public class ValidateTokenResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
