namespace GoldenFiberERP.Domain.Events.Inventory;

/// <summary>
/// Domain event raised when product stock quantity changes
/// </summary>
public sealed record ProductStockChangedEvent(
    int ProductId,
    string ProductCode,
    string ProductName,
    int PreviousQuantity,
    int NewQuantity,
    int QuantityChanged,
    string Reason,
    int? ChangedBy
) : DomainEvent
{
    /// <summary>
    /// Indicates if the stock change resulted in low stock condition
    /// </summary>
    public bool IsLowStock { get; init; }

    /// <summary>
    /// Minimum stock level for the product
    /// </summary>
    public int MinimumStockLevel { get; init; }

    /// <summary>
    /// Indicates if this is a stock increase or decrease
    /// </summary>
    public bool IsIncrease => QuantityChanged > 0;
}
