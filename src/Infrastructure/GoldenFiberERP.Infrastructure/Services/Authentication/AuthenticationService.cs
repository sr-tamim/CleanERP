using Microsoft.Extensions.Options;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.API.Configuration;
using GoldenFiberERP.Shared.Models;

namespace GoldenFiberERP.Infrastructure.Services.Authentication;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> AuthenticateAsync(string usernameOrEmail, string password, string? ipAddress = null)
    {
        try
        {
            _logger.LogInformation("Authenticating user: {UsernameOrEmail}", usernameOrEmail);

            var user = await _userRepository.GetByUserNameOrEmailAsync(usernameOrEmail);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UsernameOrEmail}", usernameOrEmail);
                return Result<AuthenticationResult>.Failure("Invalid credentials");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted login: {UserId}", user.Id);
                return Result<AuthenticationResult>.Failure("Account is disabled");
            }

            // Check if user is locked out
            if (user.IsLockedOut())
            {
                _logger.LogWarning("Locked out user attempted login: {UserId}", user.Id);
                return Result<AuthenticationResult>.Failure("Account is locked out");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
                
                // Record failed attempt
                user.RecordFailedAccess();
                
                // Check if should be locked out
                if (user.AccessFailedCount >= _securitySettings.MaxLoginAttempts)
                {
                    var lockoutEnd = DateTimeOffset.UtcNow.Add(_securitySettings.AccountLockoutDuration);
                    user.Lockout(lockoutEnd);
                    _logger.LogWarning("User locked out due to failed attempts: {UserId}", user.Id);
                }

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<AuthenticationResult>.Failure("Invalid credentials");
            }

            // Successful login
            user.RecordSuccessfulLogin(ipAddress ?? "Unknown");
            await _userRepository.UpdateAsync(user);

            // Get user roles and permissions
            var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id);
            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.Id);

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(
                user.Id,
                user.UserName,
                user.Email,
                roles.Select(r => r.Name),
                permissions.Select(p => p.Name));

            var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken(
                refreshTokenValue,
                user.Id,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                ipAddress);

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

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
                Roles = roles.Select(r => r.Name),
                Permissions = permissions.Select(p => p.Name),
                LastLoginAt = user.LastLoginAt
            };

            var result = new AuthenticationResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                RefreshTokenExpiry = refreshToken.ExpiresAt,
                User = userInfo
            };

            _logger.LogInformation("User authenticated successfully: {UserId}", user.Id);
            return Result<AuthenticationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user: {UsernameOrEmail}", usernameOrEmail);
            return Result<AuthenticationResult>.Failure("Authentication error occurred");
        }
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshTokenValue, string? ipAddress = null)
    {
        try
        {
            _logger.LogInformation("Refreshing token");

            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found");
                return Result<AuthenticationResult>.Failure("Invalid refresh token");
            }

            if (!refreshToken.IsActive)
            {
                _logger.LogWarning("Inactive refresh token used: {TokenId}", refreshToken.Id);
                return Result<AuthenticationResult>.Failure("Invalid refresh token");
            }

            var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User not found or inactive for refresh token: {UserId}", refreshToken.UserId);
                return Result<AuthenticationResult>.Failure("User not found or inactive");
            }

            // Revoke old refresh token
            refreshToken.Revoke(ipAddress);
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            // Get user roles and permissions
            var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id);
            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.Id);

            // Generate new tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(
                user.Id,
                user.UserName,
                user.Email,
                roles.Select(r => r.Name),
                permissions.Select(p => p.Name));

            var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var newRefreshToken = new RefreshToken(
                newRefreshTokenValue,
                user.Id,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                ipAddress);

            // Set replacement info
            refreshToken.Revoke(ipAddress, newRefreshTokenValue);
            await _refreshTokenRepository.UpdateAsync(refreshToken);
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            await _unitOfWork.SaveChangesAsync();

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
                Roles = roles.Select(r => r.Name),
                Permissions = permissions.Select(p => p.Name),
                LastLoginAt = user.LastLoginAt
            };

            var result = new AuthenticationResult
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshTokenValue,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                RefreshTokenExpiry = newRefreshToken.ExpiresAt,
                User = userInfo
            };

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);
            return Result<AuthenticationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Result<AuthenticationResult>.Failure("Token refresh error occurred");
        }
    }

    public async Task<Result<bool>> RevokeTokenAsync(string refreshTokenValue, string? ipAddress = null)
    {
        try
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
            if (refreshToken == null)
            {
                return Result<bool>.Failure("Invalid refresh token");
            }

            if (!refreshToken.IsActive)
            {
                return Result<bool>.Success(true); // Already revoked
            }

            refreshToken.Revoke(ipAddress);
            await _refreshTokenRepository.UpdateAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            return Result<bool>.Failure("Error revoking token");
        }
    }

    public async Task<Result<int>> RegisterAsync(string userName, string email, string firstName, string lastName, string password)
    {
        try
        {
            _logger.LogInformation("Registering new user: {UserName} ({Email})", userName, email);

            // Check if username exists
            if (await _userRepository.ExistsByUserNameAsync(userName))
            {
                return Result<int>.Failure("Username already exists");
            }

            // Check if email exists
            if (await _userRepository.ExistsByEmailAsync(email))
            {
                return Result<int>.Failure("Email already exists");
            }

            // Validate password
            var passwordValidation = _passwordService.ValidatePassword(password);
            if (!passwordValidation.IsSuccess)
            {
                return Result<int>.Failure(passwordValidation.Error);
            }

            // Hash password
            var passwordHash = _passwordService.HashPassword(password);

            // Create user
            var user = new User(userName, email, firstName, lastName, passwordHash);
            
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);
            return Result<int>.Success(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {UserName}", userName);
            return Result<int>.Failure("Registration error occurred");
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
            {
                return Result<bool>.Failure("Current password is incorrect");
            }

            // Validate new password
            var passwordValidation = _passwordService.ValidatePassword(newPassword);
            if (!passwordValidation.IsSuccess)
            {
                return Result<bool>.Failure(passwordValidation.Error);
            }

            // Hash new password
            var newPasswordHash = _passwordService.HashPassword(newPassword);
            user.ChangePassword(newPasswordHash);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return Result<bool>.Failure("Password change error occurred");
        }
    }

    public async Task<Result<bool>> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        // TODO: Implement password reset functionality with token validation
        await Task.CompletedTask;
        return Result<bool>.Failure("Password reset not implemented yet");
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email)
    {
        // TODO: Implement password reset token generation
        await Task.CompletedTask;
        return Result<string>.Failure("Password reset token generation not implemented yet");
    }

    public async Task<Result<bool>> ConfirmEmailAsync(int userId, string confirmationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // TODO: Validate confirmation token
            user.ConfirmEmail();
            
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Email confirmed for user: {UserId}", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
            return Result<bool>.Failure("Email confirmation error occurred");
        }
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(int userId)
    {
        // TODO: Implement email confirmation token generation
        await Task.CompletedTask;
        return Result<string>.Failure("Email confirmation token generation not implemented yet");
    }
}
