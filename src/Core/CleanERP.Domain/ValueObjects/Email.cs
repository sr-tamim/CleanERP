namespace CleanERP.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address
/// </summary>
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Create a new Email instance
    /// </summary>
    /// <param name="value">Email address</param>
    /// <returns>Email value object</returns>
    /// <exception cref="ArgumentException">Thrown when email format is invalid</exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty", nameof(value));

        var trimmedValue = value.Trim().ToLowerInvariant();

        if (!IsValidEmailFormat(trimmedValue))
            throw new ArgumentException("Invalid email format", nameof(value));

        return new Email(trimmedValue);
    }

    private static bool IsValidEmailFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
