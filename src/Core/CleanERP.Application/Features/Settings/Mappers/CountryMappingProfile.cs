using AutoMapper;
using CleanERP.Application.Features.Settings.DTOs;
using CleanERP.Application.Features.Settings.Commands;
using CleanERP.Domain.Entities.Settings;

namespace CleanERP.Application.Features.Settings.Mappers;

/// <summary>
/// AutoMapper profile for Country entity mappings
/// </summary>
public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        // Entity to DTO mappings
        CreateMap<Country, CountryDto>();
        
        CreateMap<Country, CountryLookupDto>();

        // DTO to Command mappings
        CreateMap<CreateCountryDto, CreateCountryCommand>();
        
        CreateMap<UpdateCountryDto, UpdateCountryCommand>();

        // Command to Entity mappings (if needed)
        CreateMap<CreateCountryCommand, Country>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
    }
}
