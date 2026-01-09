using MediatR;
using AutoMapper;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Inventory.DTOs;
using GoldenFiberERP.Domain.Interfaces.Repositories.Inventory;
using GoldenFiberERP.Domain.Specifications.Inventory;

namespace GoldenFiberERP.Application.Features.Inventory.Queries;

public record GetProductsQuery : IRequest<Result<List<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository
            .GetBySpecificationAsync(new AllProductsSpecification(), cancellationToken);

        var productDtos = _mapper.Map<List<ProductDto>>(products.ToList());

        return Result<List<ProductDto>>.Success(productDtos);
    }
}
