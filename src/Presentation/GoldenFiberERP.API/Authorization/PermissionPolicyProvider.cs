using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace GoldenFiberERP.API.Authorization;

/// <summary>
/// Custom authorization policy provider for dynamic permission-based policies
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;
    private readonly IAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _options = options.Value;
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if it's a permission policy
        if (policyName.StartsWith("Permission:"))
        {
            var permission = policyName.Substring("Permission:".Length);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Check if it's a module action policy
        if (policyName.StartsWith("ModuleAction:"))
        {
            var moduleAction = policyName.Substring("ModuleAction:".Length);
            var parts = moduleAction.Split(':', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length >= 2)
            {
                var module = parts[0];
                var action = parts[1];
                var resource = parts.Length > 2 ? parts[2] : null;
                
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ModuleActionRequirement(module, action, resource))
                    .Build();
                
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        // Check if it's a role policy
        if (policyName.StartsWith("Role:"))
        {
            var role = policyName.Substring("Role:".Length);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(role)
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}

/// <summary>
/// Permission requirement for authorization
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Module action requirement for authorization
/// </summary>
public class ModuleActionRequirement : IAuthorizationRequirement
{
    public string Module { get; }
    public string Action { get; }
    public string? Resource { get; }

    public ModuleActionRequirement(string module, string action, string? resource = null)
    {
        Module = module;
        Action = action;
        Resource = resource;
    }
}
