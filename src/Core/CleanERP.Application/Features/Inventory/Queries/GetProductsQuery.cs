using MediatR;
using AutoMapper;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;
using CleanERP.Application.Features.Inventory.DTOs;
using CleanERP.Domain.Interfaces.Repositories.Inventory;
using CleanERP.Domain.Specifications.Inventory;

namespace CleanERP.Application.Features.Inventory.Queries;

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
