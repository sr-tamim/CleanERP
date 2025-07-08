using GoldenFiberERP.Domain.Events;

namespace GoldenFiberERP.Application.Common.Interfaces;

/// <summary>
/// Service for dispatching domain events
/// </summary>
public interface IDomainEventService
{
    /// <summary>
    /// Dispatch all domain events from entities in the given context
    /// </summary>
    /// <param name="context">The database context containing entities with domain events</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchEventsAsync(IApplicationDbContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatch a specific domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatch multiple domain events
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
