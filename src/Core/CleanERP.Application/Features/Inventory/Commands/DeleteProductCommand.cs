using MediatR;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;
using CleanERP.Application.Common.Exceptions;
using CleanERP.Domain.Entities.Inventory;
using CleanERP.Domain.Interfaces.Repositories.Inventory;

namespace CleanERP.Application.Features.Inventory.Commands;

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
