using CleanERP.Domain.Exceptions;

namespace CleanERP.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary amounts with currency
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Create a new Money instance
    /// </summary>
    /// <param name="amount">Monetary amount</param>
    /// <param name="currency">Currency code (e.g., "USD", "EUR")</param>
    /// <returns>Money value object</returns>
    /// <exception cref="ArgumentException">Thrown when currency is invalid</exception>
    public static Money Create(decimal amount, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    /// <summary>
    /// Create Money from USD amount
    /// </summary>
    public static Money Usd(decimal amount) => Create(amount, "USD");

    /// <summary>
    /// Create Money from EUR amount
    /// </summary>
    public static Money Eur(decimal amount) => Create(amount, "EUR");

    /// <summary>
    /// Zero money amount
    /// </summary>
    public static Money Zero(string currency = "USD") => Create(0, currency);

    /// <summary>
    /// Add two Money amounts (must be same currency)
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtract two Money amounts (must be same currency)
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");

        return Create(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiply Money by a factor
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return Create(Amount * factor, Currency);
    }

    /// <summary>
    /// Check if amount is positive
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Check if amount is negative
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// Check if amount is zero
    /// </summary>
    public bool IsZero => Amount == 0;

    public override string ToString() => $"{Amount:N2} {Currency}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    // Operators
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Money operator *(decimal factor, Money money) => money.Multiply(factor);

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => left > right || left == right;
    public static bool operator <=(Money left, Money right) => left < right || left == right;
}
