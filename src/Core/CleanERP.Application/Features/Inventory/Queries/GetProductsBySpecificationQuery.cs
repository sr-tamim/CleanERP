using MediatR;
using Microsoft.Extensions.Logging;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;
using CleanERP.Domain.Specifications.Inventory;
using CleanERP.Domain.Interfaces.Repositories.Inventory;

namespace CleanERP.Application.Features.Inventory.Queries;

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
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductsBySpecificationQueryHandler> _logger;

    public GetProductsBySpecificationQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductsBySpecificationQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProductDto>>> Handle(GetProductsBySpecificationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing product query with specifications");

            var specification = new ProductsWithFiltersSpecification(
                category: request.Category,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                lowStockOnly: request.LowStockOnly,
                activeOnly: request.ActiveOnly,
                searchTerm: request.SearchTerm,
                skip: request.Skip,
                take: request.Take);

            var products = await _productRepository.GetBySpecificationAsync(specification, cancellationToken);

            var productDtos = products.Select(p => new ProductDto
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
            }).ToList();

            _logger.LogInformation("Retrieved {Count} products matching specifications", productDtos.Count);

            return Result<IEnumerable<ProductDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing product specification query");
            return Result<IEnumerable<ProductDto>>.Failure(new[] { "An error occurred while retrieving products" });
        }
    }
}
