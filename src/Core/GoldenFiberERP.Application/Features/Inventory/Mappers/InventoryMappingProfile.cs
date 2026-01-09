using AutoMapper;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Application.Features.Inventory.DTOs;

namespace GoldenFiberERP.Application.Features.Inventory.Mappers;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        // Add more mappings as needed
    }
}
