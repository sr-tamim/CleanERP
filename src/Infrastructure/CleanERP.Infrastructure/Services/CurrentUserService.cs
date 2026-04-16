using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
