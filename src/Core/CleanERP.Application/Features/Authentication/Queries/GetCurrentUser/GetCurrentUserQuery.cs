using Microsoft.Extensions.Logging;
using MediatR;
using CleanERP.Shared.Models;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Domain.Interfaces.Repositories.Identity;

namespace CleanERP.Application.Features.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Query to get current user information
/// </summary>
public class GetCurrentUserQuery : IRequest<Result<UserInfo>>
{
}

/// <summary>
/// Handler for get current user query
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserInfo>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserInfo>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return Result<UserInfo>.Failure("User is not authenticated");
            }

            var userId = _currentUserService.GetUserIdAsInt();
            if (!userId.HasValue)
            {
                return Result<UserInfo>.Failure("Invalid user ID");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null)
            {
                return Result<UserInfo>.Failure("User not found");
            }

            var roles = _currentUserService.Roles.ToList();
            var permissions = await _userRepository.GetUserPermissionsAsync(userId.Value, cancellationToken);

            var userInfo = new UserInfo
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Roles = roles,
                Permissions = permissions,
                LastLoginAt = user.LastLoginAt
            };

            return Result<UserInfo>.Success(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return Result<UserInfo>.Failure("An error occurred while retrieving user information");
        }
    }
}
