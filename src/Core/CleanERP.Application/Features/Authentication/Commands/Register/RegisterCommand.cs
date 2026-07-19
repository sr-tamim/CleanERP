using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Shared.Models;
using CleanERP.Application.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CleanERP.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Command to register new user
/// </summary>
public class RegisterCommand : IRequest<Result<int>>
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
/// Handler for register command
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<int>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to register user: {UserName} ({Email})", 
                request.UserName, request.Email);
            
            var result = await _authenticationService.RegisterAsync(
                request.UserName,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User {UserName} registered successfully with ID: {UserId}", 
                    request.UserName, result.Value);
            }
            else
            {
                _logger.LogWarning("Registration failed for user {UserName}: {Error}", 
                    request.UserName, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {UserName}", request.UserName);
            return Result<int>.Failure("An error occurred during registration");
        }
    }
}
