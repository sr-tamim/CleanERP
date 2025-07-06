using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Inventory.DTOs;

namespace GoldenFiberERP.Application.Features.Inventory.Queries;

public record GetProductsQuery : IRequest<Result<List<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var productDtos = _mapper.Map<List<ProductDto>>(products);

        return Result<List<ProductDto>>.Success(productDtos);
    }
}
