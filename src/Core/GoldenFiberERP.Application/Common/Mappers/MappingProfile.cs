using AutoMapper;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Application.Features.Inventory.DTOs;

namespace GoldenFiberERP.Application.Common.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        // Add more mappings as needed
    }
}
