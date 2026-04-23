using MediatR;
using Microsoft.Extensions.Logging;
using CleanERP.Domain.Events.Settings;

namespace CleanERP.Application.Features.Settings.EventHandlers;

/// <summary>
/// Sample handler for country created domain event
/// </summary>
public sealed class CountryCreatedEventHandler : INotificationHandler<CountryCreatedEvent>
{
    private readonly ILogger<CountryCreatedEventHandler> _logger;

    public CountryCreatedEventHandler(ILogger<CountryCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CountryCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Country created: {CountryId} {CountryCode} {CountryName} (Region: {Region}) by {UserId}",
            notification.CountryId,
            notification.CountryCode,
            notification.CountryName,
            notification.Region ?? "N/A",
            notification.CreatedBy);

        return Task.CompletedTask;
    }
}
