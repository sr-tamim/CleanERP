using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Domain.Specifications.Inventory;
using GoldenFiberERP.Application.Common.Extensions;

namespace GoldenFiberERP.Application.Features.Inventory.Queries;

/// <summary>
/// Query to get products using specifications
/// </summary>
public record GetProductsBySpecificationQuery : IRequest<Result<IEnumerable<ProductDto>>>
{
    public string? Category { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? LowStockOnly { get; init; }
    public bool? ActiveOnly { get; init; } = true;
    public string? SearchTerm { get; init; }
    public int? Take { get; init; }
    public int? Skip { get; init; }
}

/// <summary>
/// DTO for product information
/// </summary>
public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Cost { get; init; }
    public int StockQuantity { get; init; }
    public int MinimumStockLevel { get; init; }
    public int ReorderLevel { get; init; }
    public string Unit { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsLowStock { get; init; }
    public bool NeedsReorder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Handler demonstrating the use of specifications for complex queries
/// </summary>
public class GetProductsBySpecificationQueryHandler : IRequestHandler<GetProductsBySpecificationQuery, Result<IEnumerable<ProductDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetProductsBySpecificationQueryHandler> _logger;

    public GetProductsBySpecificationQueryHandler(
        IApplicationDbContext context,
        ILogger<GetProductsBySpecificationQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProductDto>>> Handle(GetProductsBySpecificationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing product query with specifications");

            var query = _context.Products.AsQueryable();

            // Apply specifications based on request parameters
            if (request.LowStockOnly == true)
            {
                var lowStockSpec = new LowStockProductsSpecification();
                query = query.ApplySpecification(lowStockSpec);
            }
            else if (request.ActiveOnly == true)
            {
                var activeSpec = new ActiveProductsSpecification();
                query = query.ApplySpecification(activeSpec);
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var categorySpec = new ProductsByCategorySpecification(request.Category);
                query = query.ApplySpecification(categorySpec);
            }

            // Apply price range filter
            if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
            {
                var minPrice = request.MinPrice ?? 0;
                var maxPrice = request.MaxPrice ?? decimal.MaxValue;
                var priceRangeSpec = new ProductsByPriceRangeSpecification(minPrice, maxPrice);
                query = query.ApplySpecification(priceRangeSpec);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchSpec = new ProductSearchSpecification(request.SearchTerm);
                query = query.ApplySpecification(searchSpec);
            }

            // Apply paging if specified
            if (request.Skip.HasValue)
            {
                query = query.Skip(request.Skip.Value);
            }

            if (request.Take.HasValue)
            {
                query = query.Take(request.Take.Value);
            }

            // Execute query and map to DTOs
            var products = await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SKU = p.SKU,
                    Category = p.Category,
                    Price = p.Price,
                    Cost = p.Cost,
                    StockQuantity = p.StockQuantity,
                    MinimumStockLevel = p.MinimumStockLevel,
                    ReorderLevel = p.ReorderLevel,
                    Unit = p.Unit,
                    IsActive = p.IsActive,
                    IsLowStock = p.StockQuantity <= p.MinimumStockLevel,
                    NeedsReorder = p.StockQuantity <= p.ReorderLevel,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} products matching specifications", products.Count);

            return Result<IEnumerable<ProductDto>>.Success(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing product specification query");
            return Result<IEnumerable<ProductDto>>.Failure(new[] { "An error occurred while retrieving products" });
        }
    }
}
