using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Domain.Interfaces.Repositories.Inventory;

namespace GoldenFiberERP.Application.Features.Inventory.Commands;

public record UpdateProductCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; } = null;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? SKU { get; init; } = null;
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.SKU = request.SKU;
        product.UpdatedAt = _dateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
