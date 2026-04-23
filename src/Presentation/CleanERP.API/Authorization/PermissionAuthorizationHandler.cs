using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ICleanAuthorizationService = CleanERP.Application.Common.Interfaces.IAuthorizationService;

namespace CleanERP.API.Authorization;

/// <summary>
/// Authorization handler for permission-based requirements
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICleanAuthorizationService _authorizationService;

    public PermissionAuthorizationHandler(ICleanAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user ID from claims
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        // Check if user has the required permission
        var hasPermission = await _authorizationService.HasPermissionAsync(userId, requirement.Permission);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization handler for module action-based requirements
/// </summary>
public class ModuleActionAuthorizationHandler : AuthorizationHandler<ModuleActionRequirement>
{
    private readonly ICleanAuthorizationService _authorizationService;

    public ModuleActionAuthorizationHandler(ICleanAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleActionRequirement requirement)
    {
        // Get user ID from claims
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        // Check if user has the required module action permission
        var hasPermission = await _authorizationService.CanAccessResourceAsync(
            userId, 
            requirement.Module, 
            requirement.Action, 
            requirement.Resource);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
