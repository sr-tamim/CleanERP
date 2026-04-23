using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Shared.Models;
using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Application.Features.Authentication.Queries.ValidateToken;

/// <summary>
/// Query to validate JWT token
/// </summary>
public class ValidateTokenQuery : IRequest<Result<bool>>
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Handler for validate token query
/// </summary>
public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, Result<bool>>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<ValidateTokenQueryHandler> _logger;

    public ValidateTokenQueryHandler(
        IJwtTokenService jwtTokenService,
        ILogger<ValidateTokenQueryHandler> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Result<bool>.Failure("Token is required");
            }

            var result = await _jwtTokenService.ValidateTokenAsync(request.Token);
            
            return result.IsSuccess 
                ? Result<bool>.Success(true) 
                : Result<bool>.Failure("Invalid token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Result<bool>.Failure("An error occurred while validating token");
        }
    }
}
