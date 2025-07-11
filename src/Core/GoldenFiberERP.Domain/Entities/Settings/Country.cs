using System.ComponentModel.DataAnnotations;
using GoldenFiberERP.Domain.Entities.Common;
using GoldenFiberERP.Domain.Events.Settings;

namespace GoldenFiberERP.Domain.Entities.Settings;

/// <summary>
/// Country entity for managing geographical locations and regional settings
/// </summary>
public class Country : AuditableEntity
{
    // Private constructor to enforce use of factory method
    private Country() { }

    /// <summary>
    /// Country name (e.g., "United States", "Canada")
    /// </summary>
    [StringLength(100)]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-2 code (e.g., "US", "CA")
    /// </summary>
    [StringLength(2)]
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-3 code (e.g., "USA", "CAN")
    /// </summary>
    [StringLength(3)]
    public string? Code3 { get; private set; } = null;

    /// <summary>
    /// ISO 3166-1 numeric code (e.g., "840", "124")
    /// </summary>
    [StringLength(3)]
    public string? NumericCode { get; private set; } = null;

    /// <summary>
    /// Phone country code (e.g., "+1", "+44")
    /// </summary>
    [StringLength(5)]
    public string? PhoneCode { get; private set; } = null;

    /// <summary>
    /// Capital city name
    /// </summary>
    [StringLength(100)]
    public string? Capital { get; private set; } = null;

    /// <summary>
    /// Default currency code (ISO 4217)
    /// </summary>
    [StringLength(3)]
    public string? CurrencyCode { get; private set; } = null;

    /// <summary>
    /// Default currency symbol
    /// </summary>
    [StringLength(5)]
    public string? CurrencySymbol { get; private set; } = null;

    /// <summary>
    /// Default timezone identifier
    /// </summary>
    [StringLength(50)]
    public string? TimeZone { get; private set; } = null;

    /// <summary>
    /// Region/continent name
    /// </summary>
    [StringLength(50)]
    public string? Region { get; private set; } = null;

    /// <summary>
    /// Sub-region name
    /// </summary>
    [StringLength(50)]
    public string? SubRegion { get; private set; } = null;

    /// <summary>
    /// Whether the country is currently active in the system
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; private set; } = 0;

    /// <summary>
    /// Creates a new Country instance
    /// </summary>
    public static Country Create(
        string name,
        string code,
        string? code3,
        string? numericCode,
        string? phoneCode,
        string? capital,
        string? currencyCode,
        string? currencySymbol,
        string? timeZone,
        string? region,
        string? subRegion,
        int displayOrder = 0,
        int createdBy = 0)
    {
        ValidateRequiredFields(name, code, code3, numericCode);

        var country = new Country
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Code3 = string.IsNullOrEmpty(code3) ? null : code3.Trim().ToUpperInvariant(),
            NumericCode = string.IsNullOrEmpty(numericCode) ? null : numericCode.Trim(),
            PhoneCode = string.IsNullOrEmpty(phoneCode) ? null : phoneCode.Trim(),
            Capital = string.IsNullOrEmpty(capital) ? null : capital.Trim(),
            CurrencyCode = string.IsNullOrEmpty(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant(),
            CurrencySymbol = string.IsNullOrEmpty(currencySymbol) ? null : currencySymbol.Trim(),
            TimeZone = string.IsNullOrEmpty(timeZone) ? null : timeZone.Trim(),
            Region = string.IsNullOrEmpty(region) ? null : region.Trim(),
            SubRegion = string.IsNullOrEmpty(subRegion) ? null : subRegion.Trim(),
            DisplayOrder = displayOrder,
            IsActive = true
        };

        // Raise domain event
        country.AddDomainEvent(new CountryCreatedEvent(
            country.Id,
            country.Name,
            country.Code,
            country.Region,
            createdBy));

        return country;
    }

    /// <summary>
    /// Updates country information
    /// </summary>
    public void Update(
        string name,
        string? capital,
        string? currencyCode,
        string? currencySymbol,
        string? timeZone,
        string? region,
        string? subRegion,
        int displayOrder,
        int updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name cannot be empty", nameof(name));

        var previousName = Name;

        Name = name.Trim();
        Capital = string.IsNullOrEmpty(capital) ? null : capital.Trim();
        CurrencyCode = string.IsNullOrEmpty(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant();
        CurrencySymbol = string.IsNullOrEmpty(currencySymbol) ? null : currencySymbol.Trim();
        TimeZone = string.IsNullOrEmpty(timeZone) ? null : timeZone.Trim();
        Region = string.IsNullOrEmpty(region) ? null : region.Trim();
        SubRegion = string.IsNullOrEmpty(subRegion) ? null : subRegion.Trim();
        DisplayOrder = displayOrder;

        // Raise domain event if name changed
        if (previousName != Name)
        {
            AddDomainEvent(new CountryUpdatedEvent(
                Id,
                previousName,
                Name,
                Code,
                updatedBy));
        }
    }

    /// <summary>
    /// Activates the country
    /// </summary>
    public void Activate(int updatedBy)
    {
        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(new CountryStatusChangedEvent(Id, Name, Code, true, updatedBy));
        }
    }

    /// <summary>
    /// Deactivates the country
    /// </summary>
    public void Deactivate(int updatedBy)
    {
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(new CountryStatusChangedEvent(Id, Name, Code, false, updatedBy));
        }
    }

    private static void ValidateRequiredFields(string name, string code, string? code3, string? numericCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            throw new ArgumentException("Country code must be exactly 2 characters", nameof(code));

        if (!string.IsNullOrWhiteSpace(code3) && code3.Length != 3)
            throw new ArgumentException("Country code3 must be exactly 3 characters", nameof(code3));

        if (!string.IsNullOrWhiteSpace(numericCode) && numericCode.Length != 3)
            throw new ArgumentException("Numeric code must be exactly 3 digits", nameof(numericCode));

        if (!string.IsNullOrWhiteSpace(numericCode) && !numericCode.All(char.IsDigit))
            throw new ArgumentException("Numeric code must contain only digits", nameof(numericCode));
    }
}
