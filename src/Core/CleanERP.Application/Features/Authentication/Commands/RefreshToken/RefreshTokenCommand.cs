using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Shared.Models;
using CleanERP.Application.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CleanERP.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Command to refresh access token using refresh token
/// </summary>
public class RefreshTokenCommand : IRequest<Result<AuthenticationResult>>
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    
    public string? IpAddress { get; set; }
}

/// <summary>
/// Handler for refresh token command
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to refresh token");
            
            var result = await _authenticationService.RefreshTokenAsync(
                request.RefreshToken, 
                request.IpAddress);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Token refreshed successfully");
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Error}", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Result<AuthenticationResult>.Failure("An error occurred during token refresh");
        }
    }
}
