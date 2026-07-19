namespace CleanERP.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
    string? GetClaim(string claimType);
    
    /// <summary>
    /// Get user ID as integer
    /// </summary>
    int? GetUserIdAsInt();
    
    /// <summary>
    /// Check if user is system administrator
    /// </summary>
    bool IsSystemAdmin { get; }
    
    /// <summary>
    /// Get current user's full name
    /// </summary>
    string? FullName { get; }
    
    /// <summary>
    /// Get client IP address
    /// </summary>
    string? IpAddress { get; }
}
