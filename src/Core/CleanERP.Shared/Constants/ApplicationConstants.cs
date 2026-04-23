namespace CleanERP.Shared.Constants;

public static class ApplicationConstants
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Manager = "Manager";
        public const string Employee = "Employee";
        public const string Viewer = "Viewer";
    }

    public static class Permissions
    {
        public const string CanViewProducts = "products.view";
        public const string CanCreateProducts = "products.create";
        public const string CanUpdateProducts = "products.update";
        public const string CanDeleteProducts = "products.delete";
        
        public const string CanViewInventory = "inventory.view";
        public const string CanManageInventory = "inventory.manage";
        
        public const string CanViewReports = "reports.view";
        public const string CanGenerateReports = "reports.generate";
        
        public const string CanManageUsers = "users.manage";
        public const string CanViewAuditLogs = "audit.view";
    }

    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireManagerRole = "RequireManagerRole";
        public const string RequireEmployeeRole = "RequireEmployeeRole";
    }

    public static class DefaultValues
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int DefaultTimeoutMinutes = 30;
        
        public static class Inventory
        {
            public const int LowStockThreshold = 10;
            public const int CriticalStockThreshold = 5;
            public const decimal DefaultMarkupPercentage = 25.0m;
        }
    }

    public static class Validation
    {
        public const int MinProductNameLength = 2;
        public const int MaxProductNameLength = 100;
        public const int MaxDescriptionLength = 500;
        public const int MaxSKULength = 50;
        
        public const decimal MinPrice = 0.01m;
        public const decimal MaxPrice = 999999.99m;
        
        public const int MinStockQuantity = 0;
        public const int MaxStockQuantity = 999999;
    }
}
