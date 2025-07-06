namespace GoldenFiberERP.Shared.Enums;

public enum ProductStatus
{
    Active = 1,
    Inactive = 2,
    Discontinued = 3,
    OutOfStock = 4
}

public enum InventoryTransactionType
{
    Purchase = 1,
    Sale = 2,
    Adjustment = 3,
    Transfer = 4,
    Return = 5,
    Damage = 6,
    Loss = 7
}

public enum OrderStatus
{
    Draft = 1,
    Pending = 2,
    Confirmed = 3,
    InProgress = 4,
    Shipped = 5,
    Delivered = 6,
    Completed = 7,
    Cancelled = 8,
    Returned = 9
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingActivation = 4
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    View = 4,
    Login = 5,
    Logout = 6,
    Export = 7,
    Import = 8
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}

public enum ReportType
{
    InventoryReport = 1,
    SalesReport = 2,
    PurchaseReport = 3,
    FinancialReport = 4,
    UserActivityReport = 5,
    AuditReport = 6
}
