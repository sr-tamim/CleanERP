using MediatR;
using AutoMapper;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Inventory.DTOs;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Domain.Interfaces.Repositories.Inventory;

namespace GoldenFiberERP.Application.Features.Inventory.Queries;

public record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        var productDto = _mapper.Map<ProductDto>(product);

        return Result<ProductDto>.Success(productDto);
    }
}
