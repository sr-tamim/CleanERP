using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Shared.Models;
using CleanERP.Application.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CleanERP.Application.Features.Authentication.Commands.Logout;

/// <summary>
/// Command to logout user and revoke refresh token
/// </summary>
public class LogoutCommand : IRequest<Result<bool>>
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    
    public string? IpAddress { get; set; }
}

/// <summary>
/// Handler for logout command
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<LogoutCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to logout user");
            
            var result = await _authenticationService.RevokeTokenAsync(
                request.RefreshToken, 
                request.IpAddress);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User logged out successfully");
            }
            else
            {
                _logger.LogWarning("Logout failed: {Error}", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Result<bool>.Failure("An error occurred during logout");
        }
    }
}
