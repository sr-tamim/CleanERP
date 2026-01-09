using Microsoft.Extensions.Logging;
using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Events;

namespace GoldenFiberERP.Infrastructure.Services;

/// <summary>
/// Service for dispatching domain events using MediatR
/// </summary>
public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IMediator _mediator;

    public DomainEventService(ILogger<DomainEventService> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Dispatching domain event: {EventType} with ID: {EventId}", 
                domainEvent.GetType().Name, domainEvent.EventId);

            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogDebug("Successfully dispatched domain event: {EventType} with ID: {EventId}", 
                domainEvent.GetType().Name, domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching domain event: {EventType} with ID: {EventId}", 
                domainEvent.GetType().Name, domainEvent.EventId);
            throw;
        }
    }

    public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var events = domainEvents.ToArray();
        
        if (!events.Any())
        {
            return;
        }

        foreach (var domainEvent in events)
        {
            await DispatchEventAsync(domainEvent, cancellationToken);
        }

        _logger.LogInformation("Successfully dispatched {Count} domain events", events.Length);
    }
}
