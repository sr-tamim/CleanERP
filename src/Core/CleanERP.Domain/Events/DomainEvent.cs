namespace CleanERP.Domain.Events;

/// <summary>
/// Base class for domain events with common properties
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Version of the event for versioning support
    /// </summary>
    public virtual int Version { get; } = 1;
}
