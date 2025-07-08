using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Application.Features.Inventory.Commands;

public record CreateProductCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? SKU { get; init; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public CreateProductCommandHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SKU))
        {
            return Result<int>.Failure(new[] { "SKU is required" });
        }

        var product = Product.Create(
            name: request.Name,
            description: request.Description ?? string.Empty,
            sku: request.SKU,
            category: "General", // Default category - could be added to command
            price: request.Price,
            cost: 0, // Default cost - could be added to command
            initialStock: request.StockQuantity,
            minimumStockLevel: 10, // Default minimum stock level
            reorderLevel: 20, // Default reorder level
            unit: "PCS", // Default unit
            createdBy: 0); // Default user - should come from current user service

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(product.Id);
    }
}
