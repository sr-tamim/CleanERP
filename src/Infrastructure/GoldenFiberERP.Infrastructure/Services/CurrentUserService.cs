using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using GoldenFiberERP.Application.Common.Interfaces;

namespace GoldenFiberERP.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    
    public IEnumerable<string> Permissions => _httpContextAccessor.HttpContext?.User?.FindAll("permission")?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    
    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
    
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
    
    public string? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }
    
    public int? GetUserIdAsInt()
    {
        if (int.TryParse(UserId, out var userId))
            return userId;
        return null;
    }
    
    public bool IsSystemAdmin => IsInRole("SystemAdmin") || IsInRole("SuperAdmin");
    
    public string? FullName => GetClaim("full_name") ?? $"{GetClaim("given_name")} {GetClaim("family_name")}".Trim();
    
    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}
