using MediatR;
using Microsoft.Extensions.Logging;
using CleanERP.Domain.Events.Settings;

namespace CleanERP.Application.Features.Settings.EventHandlers;

/// <summary>
/// Sample auditing handler for country created domain event
/// </summary>
public sealed class CountryCreatedAuditEventHandler : INotificationHandler<CountryCreatedEvent>
{
    private readonly ILogger<CountryCreatedAuditEventHandler> _logger;

    public CountryCreatedAuditEventHandler(ILogger<CountryCreatedAuditEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CountryCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Audit (demo): Country created {CountryId} {CountryCode} by {UserId}",
            notification.CountryId,
            notification.CountryCode,
            notification.CreatedBy);

        /*
        Example DB audit write (pseudo-code):

        var auditRecord = new AuditLog
        {
            EntityType = "Country",
            EntityId = notification.CountryId,
            Action = "Created",
            PerformedBy = notification.CreatedBy,
            PerformedAt = notification.OccurredAt,
            Metadata = new
            {
                notification.CountryName,
                notification.CountryCode,
                notification.Region
            }
        };

        await _auditLogRepository.AddAsync(auditRecord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        */

        return Task.CompletedTask;
    }
}
