using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;

namespace GoldenFiberERP.Infrastructure.Services.Authentication;

/// <summary>
/// Authorization service implementation
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        ICacheService cacheService,
        ILogger<AuthorizationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        try
        {
            var cacheKey = $"user_permission_{userId}_{permission}";
            var cachedResult = await _cacheService.GetAsync<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
            {
                return cachedResult.Value;
            }

            var hasPermission = await _permissionRepository.UserHasPermissionAsync(userId, permission);
            
            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, hasPermission, TimeSpan.FromMinutes(5));
            
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<bool> IsInRoleAsync(int userId, string role)
    {
        try
        {
            var cacheKey = $"user_role_{userId}_{role}";
            var cachedResult = await _cacheService.GetAsync<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
            {
                return cachedResult.Value;
            }

            var userRoles = await _roleRepository.GetRolesByUserIdAsync(userId);
            var isInRole = userRoles.Any(r => r.Name.Equals(role, StringComparison.OrdinalIgnoreCase));
            
            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, isInRole, TimeSpan.FromMinutes(5));
            
            return isInRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role {Role} for user {UserId}", role, userId);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        try
        {
            var cacheKey = $"user_permissions_{userId}";
            var cachedPermissions = await _cacheService.GetAsync<IEnumerable<string>>(cacheKey);
            
            if (cachedPermissions != null)
            {
                return cachedPermissions;
            }

            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(userId);
            var permissionNames = permissions.Select(p => p.Name).ToList();
            
            // Cache for 10 minutes
            await _cacheService.SetAsync(cacheKey, permissionNames, TimeSpan.FromMinutes(10));
            
            return permissionNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        try
        {
            var cacheKey = $"user_roles_{userId}";
            var cachedRoles = await _cacheService.GetAsync<IEnumerable<string>>(cacheKey);
            
            if (cachedRoles != null)
            {
                return cachedRoles;
            }

            var roles = await _roleRepository.GetRolesByUserIdAsync(userId);
            var roleNames = roles.Select(r => r.Name).ToList();
            
            // Cache for 10 minutes
            await _cacheService.SetAsync(cacheKey, roleNames, TimeSpan.FromMinutes(10));
            
            return roleNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> CanAccessResourceAsync(int userId, string module, string action, string? resource = null)
    {
        try
        {
            var cacheKey = $"user_access_{userId}_{module}_{action}_{resource ?? "null"}";
            var cachedResult = await _cacheService.GetAsync<bool?>(cacheKey);
            
            if (cachedResult.HasValue)
            {
                return cachedResult.Value;
            }

            var hasAccess = await _permissionRepository.UserHasModuleActionAsync(userId, module, action, resource);
            
            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, hasAccess, TimeSpan.FromMinutes(5));
            
            return hasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access to {Module}.{Action}.{Resource} for user {UserId}", 
                module, action, resource, userId);
            return false;
        }
    }
}
