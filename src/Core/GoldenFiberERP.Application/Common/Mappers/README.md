# AutoMapper Configuration - Golden Fiber ERP

## Overview

This document describes the enhanced AutoMapper configuration implemented for the Golden Fiber ERP system, following enterprise best practices and AutoMapper 15.x guidelines.

## Configuration Features

### 1. Enhanced Registration
- **Location**: `GoldenFiberERP.Application\DependencyInjection.cs`
- **Method**: `AddAutoMapper()` with custom configuration delegate
- **Assembly Scanning**: Automatically discovers all `Profile` classes in the Application assembly

### 2. Enterprise-Focused Settings
```csharp
cfg.AllowNullDestinationValues = false; // Prevents null assignments
cfg.AllowNullCollections = false;       // Ensures collections are initialized
```

### 3. Profile Organization
- **Main Profile**: `Common\Mappers\MappingProfile.cs` - General entity mappings
- **Feature Profiles**: `Features\{Module}\Mappers\{Entity}MappingProfile.cs` - Module-specific mappings

## Best Practices Implemented

### 1. Configuration Validation
- Extension method: `AutoMapperExtensions.ValidateAutoMapperConfiguration()`
- Validates all mappings during application startup
- Throws descriptive errors for misconfigured mappings

### 2. Profile Structure
```csharp
public class ExampleMappingProfile : Profile
{
    public ExampleMappingProfile()
    {
        // Entity to DTO
        CreateMap<Entity, EntityDto>();
        
        // Command to Entity with exclusions
        CreateMap<CreateEntityCommand, Entity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
```

### 3. Development Validation
```csharp
// In Program.cs or Startup.cs (Development only)
if (environment.IsDevelopment())
{
    serviceProvider.ValidateAutoMapperConfiguration(logger);
}
```

## Usage Examples

### 1. Controller Injection
```csharp
public class ProductController : ControllerBase
{
    private readonly IMapper _mapper;
    
    public ProductController(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public async Task<ProductDto> GetProduct(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return _mapper.Map<ProductDto>(product);
    }
}
```

### 2. Service Layer Usage
```csharp
public class ProductService
{
    private readonly IMapper _mapper;
    
    public ProductService(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        // ... business logic
        return _mapper.Map<ProductDto>(product);
    }
}
```

## Migration from Legacy Versions

### Removed Dependencies
- ❌ `AutoMapper.Extensions.Microsoft.DependencyInjection` (deprecated in v13+)

### Updated Registration
- ✅ Core package `AddAutoMapper()` method
- ✅ Configuration delegate for custom settings
- ✅ Assembly scanning for automatic profile discovery

## Performance Considerations

1. **Lazy Compilation**: Maps are compiled on first use
2. **Null Handling**: Configured to prevent null-related performance issues
3. **Collection Initialization**: Ensures collections are properly initialized

## Error Handling

- **Configuration Errors**: Caught during application startup with descriptive messages
- **Runtime Errors**: Standard AutoMapper exceptions with context
- **Validation**: `AssertConfigurationIsValid()` for development testing

## Future Enhancements

1. **Custom Type Converters**: For complex business object transformations
2. **Value Resolvers**: For calculated or composite properties
3. **Conditional Mapping**: Based on business rules or user permissions
4. **Profile Modularization**: Further organization by business domain

---

*This configuration follows AutoMapper 15.x best practices and is optimized for enterprise ERP scenarios with large datasets and complex object hierarchies.*
