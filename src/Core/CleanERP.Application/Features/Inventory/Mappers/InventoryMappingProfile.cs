using AutoMapper;
using CleanERP.Domain.Entities.Inventory;
using CleanERP.Application.Features.Inventory.DTOs;

namespace CleanERP.Application.Features.Inventory.Mappers;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
