using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Domain.Specifications.Inventory;

/// <summary>
/// Specification for products by price range
/// </summary>
public class ProductsByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice) 
        : base(p => p.Price >= minPrice && p.Price <= maxPrice && !p.IsDeleted)
    {
        AddOrderBy(p => p.Price);
    }
}

/// <summary>
/// Specification for products that need reordering
/// </summary>
public class ProductsNeedingReorderSpecification : BaseSpecification<Product>
{
    public ProductsNeedingReorderSpecification() 
        : base(p => p.StockQuantity <= p.ReorderLevel && p.IsActive && !p.IsDeleted)
    {
        AddOrderBy(p => p.StockQuantity);
        AddOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products by SKU pattern
/// </summary>
public class ProductsBySkuPatternSpecification : BaseSpecification<Product>
{
    public ProductsBySkuPatternSpecification(string skuPattern) 
        : base(p => p.SKU.Contains(skuPattern) && !p.IsDeleted)
    {
        AddOrderBy(p => p.SKU);
    }
}

/// <summary>
/// Specification for products with zero stock
/// </summary>
public class OutOfStockProductsSpecification : BaseSpecification<Product>
{
    public OutOfStockProductsSpecification() 
        : base(p => p.StockQuantity == 0 && p.IsActive && !p.IsDeleted)
    {
        AddOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products created within a date range
/// </summary>
public class ProductsCreatedInDateRangeSpecification : BaseSpecification<Product>
{
    public ProductsCreatedInDateRangeSpecification(DateTime startDate, DateTime endDate) 
        : base(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && !p.IsDeleted)
    {
        AddOrderByDescending(p => p.CreatedAt);
    }
}

/// <summary>
/// Specification for expensive products (above a threshold)
/// </summary>
public class ExpensiveProductsSpecification : BaseSpecification<Product>
{
    public ExpensiveProductsSpecification(decimal priceThreshold) 
        : base(p => p.Price > priceThreshold && !p.IsDeleted)
    {
        AddOrderByDescending(p => p.Price);
    }
}

/// <summary>
/// Specification for products with high stock levels
/// </summary>
public class HighStockProductsSpecification : BaseSpecification<Product>
{
    public HighStockProductsSpecification(int stockThreshold) 
        : base(p => p.StockQuantity > stockThreshold && !p.IsDeleted)
    {
        AddOrderByDescending(p => p.StockQuantity);
    }
}

/// <summary>
/// Combined specification for search functionality
/// </summary>
public class ProductSearchSpecification : BaseSpecification<Product>
{
    public ProductSearchSpecification(string searchTerm) 
        : base(p => (p.Name.Contains(searchTerm) || 
                    p.Description.Contains(searchTerm) || 
                    p.SKU.Contains(searchTerm) ||
                    p.Category.Contains(searchTerm)) && 
                    !p.IsDeleted)
    {
        AddOrderBy(p => p.Name);
    }
}
