using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Domain.Interfaces.Repositories.Inventory;

namespace GoldenFiberERP.Application.Features.Inventory.Commands;

public record DeleteProductCommand(int Id) : IRequest<Result>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        await _productRepository.DeleteAsync(product.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
