using GoldenFiberERP.Shared.Models;

namespace GoldenFiberERP.Application.Common.Interfaces.Identity;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> AuthenticateAsync(string userName, string password, string? ipAddress = null);
    Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<Result> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task<Result> LogoutAsync(string? refreshToken = null, string? ipAddress = null);
    Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<Result> ResetPasswordAsync(string email);
    Task<Result> ConfirmPasswordResetAsync(string token, string email, string newPassword);
}

public class AuthenticationResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}
