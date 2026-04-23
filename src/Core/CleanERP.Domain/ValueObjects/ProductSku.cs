namespace CleanERP.Domain.ValueObjects;

/// <summary>
/// Value object representing a product SKU (Stock Keeping Unit)
/// </summary>
public sealed class ProductSku : ValueObject
{
    public string Value { get; }

    private ProductSku(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Create a new ProductSku instance
    /// </summary>
    /// <param name="value">SKU value</param>
    /// <returns>ProductSku value object</returns>
    /// <exception cref="ArgumentException">Thrown when SKU format is invalid</exception>
    public static ProductSku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be null or empty", nameof(value));

        var trimmedValue = value.Trim().ToUpperInvariant();

        if (trimmedValue.Length < 3)
            throw new ArgumentException("SKU must be at least 3 characters long", nameof(value));

        if (trimmedValue.Length > 50)
            throw new ArgumentException("SKU cannot be longer than 50 characters", nameof(value));

        // Basic validation - alphanumeric characters and some special characters
        if (!IsValidSkuFormat(trimmedValue))
            throw new ArgumentException("SKU contains invalid characters. Only letters, numbers, hyphens, and underscores are allowed", nameof(value));

        return new ProductSku(trimmedValue);
    }

    private static bool IsValidSkuFormat(string value)
    {
        return value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSku sku) => sku.Value;
}
