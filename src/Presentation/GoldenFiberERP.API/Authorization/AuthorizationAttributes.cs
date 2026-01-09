using Microsoft.AspNetCore.Authorization;

namespace GoldenFiberERP.API.Authorization;

/// <summary>
/// Authorization attribute for permission-based access control
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission:{permission}";
    }
}

/// <summary>
/// Authorization attribute for module action-based access control
/// </summary>
public class RequireModuleActionAttribute : AuthorizeAttribute
{
    public RequireModuleActionAttribute(string module, string action, string? resource = null)
    {
        Policy = string.IsNullOrEmpty(resource) 
            ? $"ModuleAction:{module}:{action}"
            : $"ModuleAction:{module}:{action}:{resource}";
    }
}

/// <summary>
/// Authorization attribute for role-based access control
/// </summary>
public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(string role)
    {
        Policy = $"Role:{role}";
    }
}

/// <summary>
/// Common permission names for the ERP system
/// </summary>
public static class Permissions
{
    // User Management
    public const string UserCreate = "users.create";
    public const string UserRead = "users.read";
    public const string UserUpdate = "users.update";
    public const string UserDelete = "users.delete";
    public const string UserManage = "users.manage";

    // Role Management
    public const string RoleCreate = "roles.create";
    public const string RoleRead = "roles.read";
    public const string RoleUpdate = "roles.update";
    public const string RoleDelete = "roles.delete";
    public const string RoleManage = "roles.manage";

    // Permission Management
    public const string PermissionCreate = "permissions.create";
    public const string PermissionRead = "permissions.read";
    public const string PermissionUpdate = "permissions.update";
    public const string PermissionDelete = "permissions.delete";
    public const string PermissionManage = "permissions.manage";

    // System Administration
    public const string SystemAdmin = "system.admin";
    public const string SystemSettings = "system.settings";
    public const string SystemHealth = "system.health";
    public const string SystemLogs = "system.logs";

    // Inventory Management
    public const string InventoryCreate = "inventory.create";
    public const string InventoryRead = "inventory.read";
    public const string InventoryUpdate = "inventory.update";
    public const string InventoryDelete = "inventory.delete";
    public const string InventoryManage = "inventory.manage";

    // Product Management
    public const string ProductCreate = "products.create";
    public const string ProductRead = "products.read";
    public const string ProductUpdate = "products.update";
    public const string ProductDelete = "products.delete";
    public const string ProductManage = "products.manage";

    // Sales Management
    public const string SalesCreate = "sales.create";
    public const string SalesRead = "sales.read";
    public const string SalesUpdate = "sales.update";
    public const string SalesDelete = "sales.delete";
    public const string SalesManage = "sales.manage";

    // Purchase Management
    public const string PurchaseCreate = "purchases.create";
    public const string PurchaseRead = "purchases.read";
    public const string PurchaseUpdate = "purchases.update";
    public const string PurchaseDelete = "purchases.delete";
    public const string PurchaseManage = "purchases.manage";

    // Reports
    public const string ReportRead = "reports.read";
    public const string ReportGenerate = "reports.generate";
    public const string ReportExport = "reports.export";
}

/// <summary>
/// Common module names for the ERP system
/// </summary>
public static class Modules
{
    public const string Users = "Users";
    public const string Roles = "Roles";
    public const string Permissions = "Permissions";
    public const string System = "System";
    public const string Inventory = "Inventory";
    public const string Products = "Products";
    public const string Sales = "Sales";
    public const string Purchases = "Purchases";
    public const string Reports = "Reports";
    public const string Settings = "Settings";
}

/// <summary>
/// Common action names for the ERP system
/// </summary>
public static class Actions
{
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Manage = "Manage";
    public const string Export = "Export";
    public const string Import = "Import";
    public const string Approve = "Approve";
    public const string Reject = "Reject";
}

/// <summary>
/// Common role names for the ERP system
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Guest = "Guest";
    
    // Department-specific roles
    public const string InventoryManager = "InventoryManager";
    public const string SalesManager = "SalesManager";
    public const string PurchaseManager = "PurchaseManager";
    public const string FinanceManager = "FinanceManager";
    public const string HRManager = "HRManager";
}
