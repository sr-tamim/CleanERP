namespace GoldenFiberERP.Domain.ValueObjects;

/// <summary>
/// Value object representing a phone number
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }

    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number = number;
        Value = $"{countryCode}{number}";
    }

    /// <summary>
    /// Create a new PhoneNumber instance
    /// </summary>
    /// <param name="value">Full phone number including country code</param>
    /// <returns>PhoneNumber value object</returns>
    /// <exception cref="ArgumentException">Thrown when phone number format is invalid</exception>
    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(value));

        // Remove all non-digit characters except +
        var cleanValue = new string(value.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (string.IsNullOrEmpty(cleanValue))
            throw new ArgumentException("Phone number must contain digits", nameof(value));

        // Handle international format
        if (cleanValue.StartsWith("+"))
        {
            cleanValue = cleanValue.Substring(1);
        }

        if (cleanValue.Length < 7 || cleanValue.Length > 15)
            throw new ArgumentException("Phone number must be between 7 and 15 digits", nameof(value));

        // Extract country code (first 1-3 digits)
        var countryCode = ExtractCountryCode(cleanValue);
        var number = cleanValue.Substring(countryCode.Length);

        return new PhoneNumber($"+{countryCode}", number);
    }

    /// <summary>
    /// Create a new PhoneNumber instance with explicit country code and number
    /// </summary>
    /// <param name="countryCode">Country code (e.g., "1", "44", "49")</param>
    /// <param name="number">Phone number without country code</param>
    /// <returns>PhoneNumber value object</returns>
    public static PhoneNumber Create(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be null or empty", nameof(countryCode));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(number));

        var cleanCountryCode = new string(countryCode.Where(char.IsDigit).ToArray());
        var cleanNumber = new string(number.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(cleanCountryCode))
            throw new ArgumentException("Country code must contain digits", nameof(countryCode));

        if (string.IsNullOrEmpty(cleanNumber))
            throw new ArgumentException("Phone number must contain digits", nameof(number));

        if (cleanCountryCode.Length > 3)
            throw new ArgumentException("Country code cannot be longer than 3 digits", nameof(countryCode));

        if (cleanNumber.Length < 4 || cleanNumber.Length > 12)
            throw new ArgumentException("Phone number must be between 4 and 12 digits", nameof(number));

        return new PhoneNumber($"+{cleanCountryCode}", cleanNumber);
    }

    private static string ExtractCountryCode(string phoneNumber)
    {
        // Simple country code extraction logic
        // In a real implementation, this would use a proper country code lookup
        if (phoneNumber.Length >= 10 && (phoneNumber.StartsWith("1") || phoneNumber.StartsWith("7")))
        {
            return phoneNumber.Substring(0, 1);
        }
        
        if (phoneNumber.Length >= 9)
        {
            var twoDigit = phoneNumber.Substring(0, 2);
            if (IsValidTwoDigitCountryCode(twoDigit))
            {
                return twoDigit;
            }
        }

        if (phoneNumber.Length >= 8)
        {
            var threeDigit = phoneNumber.Substring(0, 3);
            if (IsValidThreeDigitCountryCode(threeDigit))
            {
                return threeDigit;
            }
        }

        // Default to single digit country code
        return phoneNumber.Substring(0, 1);
    }

    private static bool IsValidTwoDigitCountryCode(string code)
    {
        // Common two-digit country codes
        var validCodes = new[] { "20", "27", "30", "31", "32", "33", "34", "36", "39", "40", "41", "43", "44", "45", "46", "47", "48", "49", "51", "52", "53", "54", "55", "56", "57", "58", "60", "61", "62", "63", "64", "65", "66", "81", "82", "84", "86", "90", "91", "92", "93", "94", "95", "98" };
        return validCodes.Contains(code);
    }

    private static bool IsValidThreeDigitCountryCode(string code)
    {
        // Common three-digit country codes
        var validCodes = new[] { "212", "213", "216", "218", "220", "221", "222", "223", "224", "225", "226", "227", "228", "229", "230", "231", "232", "233", "234", "235", "236", "237", "238", "239", "240", "241", "242", "243", "244", "245", "246", "247", "248", "249", "250", "251", "252", "253", "254", "255", "256", "257", "258", "260", "261", "262", "263", "264", "265", "266", "267", "268", "269", "290", "291", "297", "298", "299" };
        return validCodes.Contains(code);
    }

    /// <summary>
    /// Get formatted phone number for display
    /// </summary>
    /// <returns>Formatted phone number</returns>
    public string GetFormattedNumber()
    {
        return $"{CountryCode} {FormatLocalNumber()}";
    }

    private string FormatLocalNumber()
    {
        // Simple formatting - in practice this would be country-specific
        if (Number.Length == 10) // US format
        {
            return $"({Number.Substring(0, 3)}) {Number.Substring(3, 3)}-{Number.Substring(6)}";
        }
        
        return Number;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
