# Clean Architecture Guidelines - Adding New Features/Modules

## Overview

This document provides a step-by-step guide for adding new features or modules to the GoldenFiberERP system following Clean Architecture principles. The system is organized into four main layers: Domain, Application, Infrastructure, and Presentation.

## Architecture Layers

```
📁 src/
├── 📁 Core/
│   ├── 📁 GoldenFiberERP.Domain/          # Business Logic & Entities
│   ├── 📁 GoldenFiberERP.Application/     # Application Logic & Use Cases
│   └── 📁 GoldenFiberERP.Shared/          # Shared Utilities
├── 📁 Infrastructure/
│   ├── 📁 GoldenFiberERP.Infrastructure/  # External Services
│   └── 📁 GoldenFiberERP.Persistence/     # Database & Repository Implementations
└── 📁 Presentation/
    └── 📁 GoldenFiberERP.API/             # API Controllers & Configuration
```

## Step-by-Step Implementation Guide

### 1. Domain Layer Implementation

#### 1.1 Create Domain Entity

**Location:** `src/Core/GoldenFiberERP.Domain/Entities/{ModuleName}/{EntityName}.cs`

**Example:** `src/Core/GoldenFiberERP.Domain/Entities/Settings/Country.cs`

```csharp
using GoldenFiberERP.Domain.Entities.Common;
using GoldenFiberERP.Domain.Events.Settings;

namespace GoldenFiberERP.Domain.Entities.Settings;

public class Country : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    // ... other properties

    private Country() { } // EF Constructor

    // Factory method for creating new entities
    public static Country Create(
        string name,
        string code,
        // ... other parameters
        int createdBy)
    {
        var country = new Country
        {
            Name = name,
            Code = code,
            // ... set other properties
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Add domain event
        country.AddDomainEvent(new CountryCreatedEvent(country));
        return country;
    }

    // Business logic methods
    public void UpdateDetails(string name, string capital, /* other params */)
    {
        Name = name;
        // ... update other properties
        AddDomainEvent(new CountryUpdatedEvent(this));
    }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new CountryActivatedEvent(this));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new CountryDeactivatedEvent(this));
    }
}
```

#### 1.2 Create Domain Events

**Location:** `src/Core/GoldenFiberERP.Domain/Events/{ModuleName}/{EntityName}Events.cs`

```csharp
using GoldenFiberERP.Domain.Events.Common;
using GoldenFiberERP.Domain.Entities.Settings;

namespace GoldenFiberERP.Domain.Events.Settings;

public record CountryCreatedEvent(Country Country) : IDomainEvent;
public record CountryUpdatedEvent(Country Country) : IDomainEvent;
public record CountryDeletedEvent(int CountryId) : IDomainEvent;
public record CountryActivatedEvent(Country Country) : IDomainEvent;
public record CountryDeactivatedEvent(Country Country) : IDomainEvent;
```

#### 1.3 Create Repository Interface

**Location:** `src/Core/GoldenFiberERP.Domain/Interfaces/Repositories/{ModuleName}/I{EntityName}Repository.cs`

```csharp
using GoldenFiberERP.Domain.Entities.Settings;

namespace GoldenFiberERP.Domain.Interfaces.Repositories.Settings;

public interface ICountryRepository
{
    Task<IEnumerable<Country>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Country?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Country>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Country country, CancellationToken cancellationToken = default);
    Task UpdateAsync(Country country, CancellationToken cancellationToken = default);
    Task DeleteAsync(Country country, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Country> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, bool? isActive,
        CancellationToken cancellationToken = default);
}
```

### 2. Application Layer Implementation

#### 2.1 Create DTOs

**Location:** `src/Core/GoldenFiberERP.Application/Features/{ModuleName}/DTOs/{EntityName}DTOs.cs`

```csharp
namespace GoldenFiberERP.Application.Features.Settings.DTOs;

// Response DTOs
public record CountryDto(
    int Id,
    string Name,
    string Code,
    string Code3,
    string Capital,
    string Region,
    bool IsActive,
    DateTime CreatedAt);

public record CountryLookupDto(int Id, string Name, string Code);

// Request DTOs
public record CreateCountryDto(
    string Name,
    string Code,
    string Code3,
    string NumericCode,
    string PhoneCode,
    string Capital,
    string CurrencyCode,
    string Region,
    int DisplayOrder);

public record UpdateCountryDto(
    string Name,
    string Capital,
    string CurrencyCode,
    string Region,
    int DisplayOrder,
    bool IsActive);
```

#### 2.2 Create Commands and Handlers

**Location:** `src/Core/GoldenFiberERP.Application/Features/{ModuleName}/Commands/`

```csharp
// CreateCountryCommand.cs
using MediatR;
using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.Application.Features.Settings.Commands;

public record CreateCountryCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    // ... other properties
}

public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommand, Result<int>>
{
    private readonly ICountryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCountryCommandHandler(ICountryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (await _repository.ExistsByCodeAsync(request.Code, cancellationToken))
        {
            return Result<int>.Failure($"Country with code '{request.Code}' already exists.");
        }

        // Create entity
        var country = Country.Create(
            request.Name,
            request.Code,
            // ... other parameters
            createdBy: 0); // Get from current user service

        // Save to repository
        await _repository.AddAsync(country, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(country.Id);
    }
}
```

#### 2.3 Create Queries and Handlers

**Location:** `src/Core/GoldenFiberERP.Application/Features/{ModuleName}/Queries/`

```csharp
// GetCountriesQuery.cs
using MediatR;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Settings.DTOs;

namespace GoldenFiberERP.Application.Features.Settings.Queries;

public record GetCountriesQuery : IRequest<Result<PagedResult<CountryDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public string SortBy { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
}

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<PagedResult<CountryDto>>>
{
    private readonly ICountryRepository _repository;
    private readonly IMapper _mapper;

    public GetCountriesQueryHandler(ICountryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);

        var mappedItems = _mapper.Map<IEnumerable<CountryDto>>(items);

        var result = new PagedResult<CountryDto>(
            mappedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PagedResult<CountryDto>>.Success(result);
    }
}
```

#### 2.4 Create Validators

**Location:** `src/Core/GoldenFiberERP.Application/Features/{ModuleName}/Validators/`

```csharp
using FluentValidation;
using GoldenFiberERP.Application.Features.Settings.Commands;

namespace GoldenFiberERP.Application.Features.Settings.Validators;

public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Country name is required")
            .MaximumLength(100).WithMessage("Country name must not exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Country code is required")
            .Length(2).WithMessage("Country code must be exactly 2 characters")
            .Matches("^[A-Z]{2}$").WithMessage("Country code must contain only uppercase letters");

        // Add more validation rules...
    }
}
```

#### 2.5 Create Mapping Profile

**Location:** `src/Core/GoldenFiberERP.Application/Features/{ModuleName}/Mappers/{EntityName}MappingProfile.cs`

```csharp
using AutoMapper;
using GoldenFiberERP.Domain.Entities.Settings;
using GoldenFiberERP.Application.Features.Settings.DTOs;

namespace GoldenFiberERP.Application.Features.Settings.Mappers;

public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Country, CountryDto>();
        CreateMap<Country, CountryLookupDto>();
        // Add reverse mappings if needed
    }
}
```

#### 2.6 Update Persistence DbContext (if needed)

**Location:** `src/Infrastructure/GoldenFiberERP.Persistence/Contexts/ApplicationDbContext.cs`

Add a new `DbSet<T>` for the entity if the module requires it.

### 3. Infrastructure Layer Implementation

#### 3.1 Create Repository Implementation

**Location:** `src/Infrastructure/GoldenFiberERP.Persistence/Repositories/{ModuleName}/{EntityName}Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Settings;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Persistence.Repositories.Settings;

public class CountryRepository : ICountryRepository
{
    private readonly ApplicationDbContext _context;

    public CountryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Countries.ToListAsync(cancellationToken);
    }

    public async Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Countries.FindAsync(new object[] { id }, cancellationToken);
    }

    // Implement other repository methods...
}
```

#### 3.2 Create EF Configuration

**Location:** `src/Infrastructure/GoldenFiberERP.Persistence/Configurations/{ModuleName}/{EntityName}Configuration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoldenFiberERP.Domain.Entities.Settings;

namespace GoldenFiberERP.Persistence.Configurations.Settings;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasIndex(c => c.Name);

        // Configure other properties...
    }
}
```

#### 3.3 Update Application DbContext

**Location:** `src/Infrastructure/GoldenFiberERP.Persistence/Contexts/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    // ... existing code ...

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Country> Countries => Set<Country>(); // Add new DbSet

    // ... rest of the implementation ...
}
```

#### 3.4 Register Repository in DI

**Location:** `src/Infrastructure/GoldenFiberERP.Persistence/DependencyInjection.cs`

```csharp
public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing registrations ...

    // Register repositories
    services.AddScoped<ICountryRepository, CountryRepository>(); // Add new repository

    // ... rest of the registrations ...
}
```

### 4. Presentation Layer Implementation

#### 4.1 Create API Controller

**Location:** `src/Presentation/GoldenFiberERP.API/Controllers/{ModuleName}/{EntityName}Controller.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using MediatR;
using GoldenFiberERP.Application.Features.Settings.Commands;
using GoldenFiberERP.Application.Features.Settings.Queries;
using GoldenFiberERP.Application.Features.Settings.DTOs;

namespace GoldenFiberERP.API.Controllers.Settings;

/// <summary>
/// Country management endpoints for geographical settings
/// </summary>
[ApiController]
[Route("api/settings/[controller]")]
[Tags("Settings - Countries")]
public class CountriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(IMediator mediator, ILogger<CountriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all countries with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCountries([FromQuery] GetCountriesQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Countries retrieved successfully"
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = "Failed to retrieve countries"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting countries");
            return StatusCode(500, new { success = false, message = "An internal server error occurred" });
        }
    }

    /// <summary>
    /// Create a new country
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDto dto)
    {
        try
        {
            var command = new CreateCountryCommand
            {
                Name = dto.Name,
                Code = dto.Code,
                // ... map other properties
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return CreatedAtAction(
                    nameof(GetCountry),
                    new { id = result.Data },
                    new { success = true, data = new { id = result.Data }, message = "Country created successfully" });
            }

            return BadRequest(new { success = false, errors = result.Errors, message = "Failed to create country" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating country");
            return StatusCode(500, new { success = false, message = "An internal server error occurred" });
        }
    }

    // Implement other CRUD endpoints...
}
```

#### 4.2 Update Swagger Configuration

**Location:** `src/Presentation/GoldenFiberERP.API/Extensions/SwaggerExtensions.cs`

Add your new controller to the appropriate module classification:

```csharp
private static bool IsSettingsController(string? controllerName)
{
    var settingsControllers = new[]
    {
        "Settings",
        "Countries", // Add your new controller
        "YourNewController", // Add here
        // ... other controllers
    };

    return settingsControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
}
```

## Database Schema Management

Since the system handles database schema manually (no migrations), you'll need to:

1. **Create the database table manually** with the structure defined in your EF configuration
2. **Ensure proper indexes** are created as specified in the configuration
3. **Add initial seed data** if required (through database scripts, not code seeding)

## Best Practices

### 1. Naming Conventions
- **Entities:** PascalCase (e.g., `Country`, `Product`)
- **Properties:** PascalCase (e.g., `Name`, `Code`)
- **Methods:** PascalCase (e.g., `GetByIdAsync`, `CreateCountry`)
- **Variables:** camelCase (e.g., `countryRepository`, `result`)

### 2. Error Handling
- Use `Result<T>` pattern for application layer operations
- Implement proper exception handling in controllers
- Log errors appropriately with context

### 3. Validation
- Use FluentValidation for input validation
- Validate at the application layer (commands/queries)
- Include business rule validations in domain entities

### 4. Security
- Implement proper authorization on controllers
- Validate user permissions before operations
- Sanitize inputs and outputs

### 5. Testing
- Write unit tests for domain logic
- Write integration tests for repository implementations
- Write API tests for controller endpoints

### 6. Performance
- Use async/await patterns consistently
- Implement pagination for list operations
- Consider caching for frequently accessed data
- Use appropriate database indexes

## File Checklist

When adding a new feature, ensure you create/update these files:

### Domain Layer
- [ ] Entity class (`Entities/{Module}/{Entity}.cs`)
- [ ] Domain events (`Events/{Module}/{Entity}Events.cs`)
- [ ] Repository interface (`Interfaces/Repositories/{Module}/I{Entity}Repository.cs`)

### Application Layer
- [ ] DTOs (`Features/{Module}/DTOs/{Entity}DTOs.cs`)
- [ ] Commands and handlers (`Features/{Module}/Commands/`)
- [ ] Queries and handlers (`Features/{Module}/Queries/`)
- [ ] Validators (`Features/{Module}/Validators/`)
- [ ] Mapping profiles (`Features/{Module}/Mappers/{Entity}MappingProfile.cs`)
- [ ] Update `ApplicationDbContext` (Persistence) for new DbSet if needed

### Infrastructure Layer
- [ ] Repository implementation (`Persistence/Repositories/{Module}/{Entity}Repository.cs`)
- [ ] EF configuration (`Configurations/{Module}/{Entity}Configuration.cs`)
- [ ] Update `ApplicationDbContext`
- [ ] Update `DependencyInjection.cs`

### Presentation Layer
- [ ] API controller (`Controllers/{Module}/{Entity}Controller.cs`)
- [ ] Update Swagger configuration if needed

### Database
- [ ] Create database table manually
- [ ] Add necessary indexes
- [ ] Add seed data if required

## Example Implementation Reference

Refer to the Country feature implementation as a complete example:
- Domain: `src/Core/GoldenFiberERP.Domain/Entities/Settings/Country.cs`
- Application: `src/Core/GoldenFiberERP.Application/Features/Settings/`
- Persistence: `src/Infrastructure/GoldenFiberERP.Persistence/Repositories/Settings/CountryRepository.cs`
- Presentation: `src/Presentation/GoldenFiberERP.API/Controllers/Settings/CountriesController.cs`

Following this pattern ensures consistency across the codebase and maintains the Clean Architecture principles.
