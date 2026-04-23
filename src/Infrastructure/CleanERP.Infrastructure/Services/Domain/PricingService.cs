using CleanERP.Domain.Services.Pricing;
using CleanERP.Domain.ValueObjects;

namespace CleanERP.Infrastructure.Services.Domain;

/// <summary>
/// Implementation of pricing domain service
/// </summary>
public class PricingService : IPricingService
{
    // Configuration values - in a real implementation these would come from configuration
    private const decimal VOLUME_DISCOUNT_THRESHOLD = 100;
    private const decimal VOLUME_DISCOUNT_PERCENTAGE = 0.05m; // 5%
    private const decimal BULK_DISCOUNT_THRESHOLD = 500;
    private const decimal BULK_DISCOUNT_PERCENTAGE = 0.10m; // 10%
    private const decimal MINIMUM_MARKUP_PERCENTAGE = 0.05m; // 5% minimum markup

    public Money CalculatePrice(Money basePrice, int customerId, int quantity, string currency = "USD")
    {
        if (basePrice.Currency != currency)
        {
            throw new ArgumentException($"Base price currency ({basePrice.Currency}) does not match requested currency ({currency})");
        }

        // Start with base price
        var unitPrice = basePrice;

        // Apply quantity discounts
        unitPrice = CalculateQuantityDiscount(unitPrice, quantity);

        // Apply customer-specific pricing (placeholder - would use customer data)
        unitPrice = CalculateCustomerDiscount(unitPrice, customerId);

        return unitPrice;
    }

    public Money CalculateDiscount(Money subtotal, int customerId)
    {
        // Calculate customer-specific discount
        var discountPercentage = GetCustomerDiscountPercentage(customerId);
        var discountAmount = subtotal.Amount * discountPercentage;
        
        return Money.Create(discountAmount, subtotal.Currency);
    }

    public Money CalculateQuantityDiscount(Money basePrice, int quantity)
    {
        if (quantity >= BULK_DISCOUNT_THRESHOLD)
        {
            // Apply bulk discount
            var discountAmount = basePrice.Amount * BULK_DISCOUNT_PERCENTAGE;
            return Money.Create(basePrice.Amount - discountAmount, basePrice.Currency);
        }
        
        if (quantity >= VOLUME_DISCOUNT_THRESHOLD)
        {
            // Apply volume discount
            var discountAmount = basePrice.Amount * VOLUME_DISCOUNT_PERCENTAGE;
            return Money.Create(basePrice.Amount - discountAmount, basePrice.Currency);
        }

        return basePrice;
    }

    public bool ValidatePrice(Money price, Money cost)
    {
        if (price.Currency != cost.Currency)
        {
            throw new ArgumentException("Price and cost must be in the same currency");
        }

        // Ensure minimum markup
        var minimumPrice = cost.Amount * (1 + MINIMUM_MARKUP_PERCENTAGE);
        return price.Amount >= minimumPrice;
    }

    public decimal CalculateMarkupPercentage(Money sellingPrice, Money cost)
    {
        if (cost.Currency != sellingPrice.Currency)
        {
            throw new ArgumentException("Selling price and cost must be in the same currency");
        }

        if (cost.Amount == 0)
        {
            throw new ArgumentException("Cost cannot be zero");
        }

        return (sellingPrice.Amount - cost.Amount) / cost.Amount;
    }

    public decimal CalculateProfitMargin(Money sellingPrice, Money cost)
    {
        if (cost.Currency != sellingPrice.Currency)
        {
            throw new ArgumentException("Selling price and cost must be in the same currency");
        }

        if (sellingPrice.Amount == 0)
        {
            throw new ArgumentException("Selling price cannot be zero");
        }

        return (sellingPrice.Amount - cost.Amount) / sellingPrice.Amount;
    }

    private Money CalculateCustomerDiscount(Money unitPrice, int customerId)
    {
        // Placeholder for customer-specific pricing logic
        // In a real implementation, this would query customer data
        var discountPercentage = GetCustomerDiscountPercentage(customerId);
        
        if (discountPercentage > 0)
        {
            var discountAmount = unitPrice.Amount * discountPercentage;
            return Money.Create(unitPrice.Amount - discountAmount, unitPrice.Currency);
        }

        return unitPrice;
    }

    private decimal GetCustomerDiscountPercentage(int customerId)
    {
        // Placeholder logic - in reality this would be based on customer tier, history, etc.
        // For demonstration purposes, returning a simple calculation
        return customerId switch
        {
            < 100 => 0.02m,   // 2% for new customers
            < 1000 => 0.05m,  // 5% for regular customers
            _ => 0.10m        // 10% for premium customers
        };
    }
}
