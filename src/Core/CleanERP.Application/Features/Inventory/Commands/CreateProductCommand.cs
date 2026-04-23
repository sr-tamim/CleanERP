using MediatR;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;
using CleanERP.Domain.Entities.Inventory;
using CleanERP.Domain.Interfaces.Repositories.Inventory;

namespace CleanERP.Application.Features.Inventory.Commands;

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
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
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

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(product.Id);
    }
}
