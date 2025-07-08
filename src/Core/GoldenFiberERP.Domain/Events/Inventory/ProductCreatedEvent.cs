namespace GoldenFiberERP.Domain.Events.Inventory;

/// <summary>
/// Domain event raised when a new product is created
/// </summary>
public sealed record ProductCreatedEvent(
    int ProductId,
    string ProductCode,
    string ProductName,
    string Category,
    decimal Price,
    int InitialStockQuantity,
    int CreatedBy
) : DomainEvent;
