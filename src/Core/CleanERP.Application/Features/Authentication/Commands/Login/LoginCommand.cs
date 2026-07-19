using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Shared.Models;
using CleanERP.Application.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CleanERP.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Command to authenticate user and generate tokens
/// </summary>
public class LoginCommand : IRequest<Result<AuthenticationResult>>
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? IpAddress { get; set; }
    
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Handler for login command
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {UsernameOrEmail}", request.UsernameOrEmail);
            
            var result = await _authenticationService.AuthenticateAsync(
                request.UsernameOrEmail, 
                request.Password, 
                request.IpAddress);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User {UsernameOrEmail} logged in successfully", request.UsernameOrEmail);
            }
            else
            {
                _logger.LogWarning("Login failed for user {UsernameOrEmail}: {Error}", 
                    request.UsernameOrEmail, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {UsernameOrEmail}", request.UsernameOrEmail);
            return Result<AuthenticationResult>.Failure("An error occurred during authentication");
        }
    }
}
