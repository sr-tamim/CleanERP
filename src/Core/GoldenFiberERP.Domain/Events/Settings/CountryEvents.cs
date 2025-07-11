namespace GoldenFiberERP.Domain.Events.Settings;

/// <summary>
/// Domain event raised when a new country is created
/// </summary>
public sealed record CountryCreatedEvent(
    int CountryId,
    string CountryName,
    string CountryCode,
    string? Region,
    int CreatedBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a country is updated
/// </summary>
public sealed record CountryUpdatedEvent(
    int CountryId,
    string PreviousName,
    string NewName,
    string CountryCode,
    int UpdatedBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a country's status changes
/// </summary>
public sealed record CountryStatusChangedEvent(
    int CountryId,
    string CountryName,
    string CountryCode,
    bool IsActive,
    int UpdatedBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a country is deleted
/// </summary>
public sealed record CountryDeletedEvent(
    int CountryId,
    string CountryName,
    string CountryCode,
    int DeletedBy
) : DomainEvent;
