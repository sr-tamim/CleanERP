using AutoMapper;
using CleanERP.Domain.Entities.Inventory;
using CleanERP.Application.Features.Inventory.DTOs;

namespace CleanERP.Application.Features.Inventory.Mappers;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        CreateMap<Product, ProductDto>();

        // Product has a private constructor and factory method — only map DTO→DTO or use the factory in command handlers
        CreateMap<CreateProductDto, Product>().IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<UpdateProductDto, Product>().IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
