namespace GoldenFiberERP.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string PostalCode { get; }

    private Address(string street, string city, string state, string country, string postalCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    /// <summary>
    /// Create a new Address instance
    /// </summary>
    /// <param name="street">Street address</param>
    /// <param name="city">City name</param>
    /// <param name="state">State or province</param>
    /// <param name="country">Country name</param>
    /// <param name="postalCode">Postal or ZIP code</param>
    /// <returns>Address value object</returns>
    /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
    public static Address Create(string street, string city, string state, string country, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));

        return new Address(
            street.Trim(),
            city.Trim(),
            state?.Trim() ?? string.Empty,
            country.Trim(),
            postalCode.Trim()
        );
    }

    /// <summary>
    /// Get the full formatted address
    /// </summary>
    public string GetFullAddress()
    {
        var parts = new List<string> { Street, City };
        
        if (!string.IsNullOrEmpty(State))
            parts.Add(State);
        
        parts.Add(PostalCode);
        parts.Add(Country);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Check if this is a valid address (basic validation)
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Street) &&
               !string.IsNullOrWhiteSpace(City) &&
               !string.IsNullOrWhiteSpace(Country) &&
               !string.IsNullOrWhiteSpace(PostalCode) &&
               PostalCode.Length >= 3; // Basic postal code validation
    }

    public override string ToString() => GetFullAddress();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street.ToUpperInvariant();
        yield return City.ToUpperInvariant();
        yield return State.ToUpperInvariant();
        yield return Country.ToUpperInvariant();
        yield return PostalCode.ToUpperInvariant();
    }
}
