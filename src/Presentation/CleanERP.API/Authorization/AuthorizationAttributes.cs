using Microsoft.AspNetCore.Authorization;

namespace CleanERP.API.Attributes;

/// <summary>
/// Attribute to specify required permission for an action
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }
    
    public RequirePermissionAttribute(string permission) : base("Permission")
    {
        Permission = permission;
    }
}

/// <summary>
/// Attribute to specify required module access for an action
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireModuleAccessAttribute : AuthorizeAttribute
{
    public string Module { get; }
    public string Action { get; }
    public string? Resource { get; }
    
    public RequireModuleAccessAttribute(string module, string action, string? resource = null) : base("ModuleAccess")
    {
        Module = module;
        Action = action;
        Resource = resource;
    }
}

/// <summary>
/// Attribute to specify required role for an action
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles) : base()
    {
        Roles = string.Join(",", roles);
    }
}
