using GoldenFiberERP.Shared.Models;

namespace GoldenFiberERP.Application.Common.Interfaces;

/// <summary>
/// JWT token service interface for token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate access token for user
    /// </summary>
    string GenerateAccessToken(int userId, string userName, string email, IEnumerable<string> roles, IEnumerable<string> permissions);
    
    /// <summary>
    /// Generate refresh token for user
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Get principal from token without validation
    /// </summary>
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromToken(string token);
    
    /// <summary>
    /// Validate token and return claims
    /// </summary>
    Task<Result<System.Security.Claims.ClaimsPrincipal>> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Get user ID from token
    /// </summary>
    int? GetUserIdFromToken(string token);
    
    /// <summary>
    /// Check if token is expired
    /// </summary>
    bool IsTokenExpired(string token);
}

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate user with username/email and password
    /// </summary>
    Task<Result<AuthenticationResult>> AuthenticateAsync(string usernameOrEmail, string password, string? ipAddress = null);
    
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    
    /// <summary>
    /// Revoke refresh token
    /// </summary>
    Task<Result<bool>> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    
    /// <summary>
    /// Register new user
    /// </summary>
    Task<Result<int>> RegisterAsync(string userName, string email, string firstName, string lastName, string password);
    
    /// <summary>
    /// Change user password
    /// </summary>
    Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    
    /// <summary>
    /// Reset user password
    /// </summary>
    Task<Result<bool>> ResetPasswordAsync(string email, string resetToken, string newPassword);
    
    /// <summary>
    /// Generate password reset token
    /// </summary>
    Task<Result<string>> GeneratePasswordResetTokenAsync(string email);
    
    /// <summary>
    /// Confirm user email
    /// </summary>
    Task<Result<bool>> ConfirmEmailAsync(int userId, string confirmationToken);
    
    /// <summary>
    /// Generate email confirmation token
    /// </summary>
    Task<Result<string>> GenerateEmailConfirmationTokenAsync(int userId);
}

/// <summary>
/// Authorization service interface
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Check if user has permission
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permission);
    
    /// <summary>
    /// Check if user is in role
    /// </summary>
    Task<bool> IsInRoleAsync(int userId, string role);
    
    /// <summary>
    /// Get user permissions
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
    
    /// <summary>
    /// Get user roles
    /// </summary>
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    
    /// <summary>
    /// Check if user can access resource
    /// </summary>
    Task<bool> CanAccessResourceAsync(int userId, string module, string action, string? resource = null);
}

/// <summary>
/// Password service interface
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hash password
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Verify password against hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Validate password strength
    /// </summary>
    Result<bool> ValidatePassword(string password);
    
    /// <summary>
    /// Generate random password
    /// </summary>
    string GenerateRandomPassword(int length = 12);
}

/// <summary>
/// Authentication result
/// </summary>
public class AuthenticationResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// User information
/// </summary>
public class UserInfo
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
    public DateTime? LastLoginAt { get; set; }
}
