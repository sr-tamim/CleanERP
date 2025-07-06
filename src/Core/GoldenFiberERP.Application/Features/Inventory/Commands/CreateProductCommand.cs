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
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            SKU = request.SKU,
            CreatedAt = _dateTime.UtcNow,
            UpdatedAt = _dateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(product.Id);
    }
}
