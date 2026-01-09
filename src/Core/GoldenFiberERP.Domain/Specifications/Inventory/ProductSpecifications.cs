using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Domain.Specifications.Inventory;

/// <summary>
/// Specification for products with low stock levels
/// </summary>
public class LowStockProductsSpecification : BaseSpecification<Product>
{
    public LowStockProductsSpecification() 
        : base(p => p.StockQuantity <= p.MinimumStockLevel && !p.IsDeleted)
    {
        AddOrderBy(p => p.StockQuantity);
        AddInclude(p => p.Category);
    }
}

/// <summary>
/// Specification for active products
/// </summary>
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification() 
        : base(p => p.IsActive && !p.IsDeleted)
    {
        AddOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products by category
/// </summary>
public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(string category) 
        : base(p => p.Category == category && !p.IsDeleted)
    {
        AddOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products with search term
/// </summary>
public class ProductsWithSearchSpecification : BaseSpecification<Product>
{
    public ProductsWithSearchSpecification(string searchTerm) 
        : base(p => !p.IsDeleted && 
                   (p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm)))
    {
        AddOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products with pagination and filtering
/// </summary>
public class ProductsWithFiltersSpecification : BaseSpecification<Product>
{
    public ProductsWithFiltersSpecification(
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? lowStockOnly = null,
        bool? activeOnly = null,
        string? searchTerm = null,
        int? skip = null,
        int? take = null)
        : base(BuildCriteria(category, minPrice, maxPrice, lowStockOnly, activeOnly, searchTerm))
    {
        AddOrderBy(p => p.Name);
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }

    private static System.Linq.Expressions.Expression<Func<Product, bool>>? BuildCriteria(
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        bool? lowStockOnly,
        bool? activeOnly,
        string? searchTerm)
    {
        return p => !p.IsDeleted &&
                   (activeOnly != true || p.IsActive) &&
                   (string.IsNullOrEmpty(category) || p.Category == category) &&
                   (!minPrice.HasValue || p.Price >= minPrice.Value) &&
                   (!maxPrice.HasValue || p.Price <= maxPrice.Value) &&
                   (!lowStockOnly.GetValueOrDefault() || p.StockQuantity <= p.MinimumStockLevel) &&
                   (string.IsNullOrEmpty(searchTerm) ||
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm) ||
                    p.Category.Contains(searchTerm));
    }
}
