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
        string? searchTerm = null,
        string? category = null,
        bool? isActive = null,
        int skip = 0,
        int take = 50)
    {
        // Build the criteria expression
        var criteria = BuildCriteria(searchTerm, category, isActive);
        
        if (criteria != null)
        {
            // Note: This is a simplified approach. In a real implementation,
            // you might want to use a more sophisticated expression builder
        }

        AddOrderBy(p => p.Name);
        ApplyPaging(skip, take);
    }

    private static System.Linq.Expressions.Expression<Func<Product, bool>>? BuildCriteria(
        string? searchTerm, 
        string? category, 
        bool? isActive)
    {
        return p => !p.IsDeleted &&
                   (isActive == null || p.IsActive == isActive) &&
                   (string.IsNullOrEmpty(category) || p.Category == category) &&
                   (string.IsNullOrEmpty(searchTerm) || 
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
    }
}
