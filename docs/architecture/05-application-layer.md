# Application Layer

## Overview

The Application layer orchestrates the domain objects to perform specific use cases and business workflows. It contains the application services, use cases, DTOs, and coordinates the interaction between the presentation layer and the domain layer.

## Design Principles

### 1. Use Case Driven
- Each use case represents a specific business scenario
- Clear input/output contracts
- Single responsibility per use case
- Orchestrates domain objects to fulfill business requirements

### 2. Framework Independence
- No direct dependencies on external frameworks
- Pure business logic coordination
- Interface-based external dependencies
- Testable without external infrastructure

### 3. Transaction Management
- Coordinates multi-entity operations
- Ensures data consistency
- Handles business transaction boundaries
- Manages unit of work patterns

## Current Implementation

### Project Structure
**Location**: `src/Core/GoldenFiberERP.Application`

**Current Status**: Implemented with CQRS pattern and feature-based organization
**Actual Structure**:
```
GoldenFiberERP.Application/
├── GoldenFiberERP.Application.csproj
├── DependencyInjection.cs     # Service registration
├── README.md                  # Application documentation
├── Common/                    # Common application concerns
│   ├── Interfaces/            # Application interfaces
│   │   └── IUnitOfWork.cs           # Transaction and save interface
│   ├── Models/                # Common models and DTOs
│   ├── Exceptions/            # Application-specific exceptions
│   └── Behaviors/             # Cross-cutting behaviors
├── Features/                  # Feature-based organization
│   ├── Health/                # Health check features
│   ├── Inventory/             # Inventory management features
│   └── Settings/              # Settings management features
│       ├── Commands/          # Command handlers (Create, Update, Delete)
│       ├── Queries/           # Query handlers (Get, List, Search)
│       ├── DTOs/              # Data transfer objects
│       ├── Validators/        # FluentValidation validators
│       └── Mappers/           # AutoMapper profiles
└── Services/                  # Application services
```

### Implemented Features

#### Settings Module - Country Management
The Country feature demonstrates the full CQRS implementation:

**Commands**:
- `CreateCountryCommand` - Creates a new country
- `UpdateCountryCommand` - Updates existing country
- `DeleteCountryCommand` - Soft deletes a country
- `ActivateCountryCommand` - Activates a country
- `DeactivateCountryCommand` - Deactivates a country

**Queries**:
- `GetCountriesQuery` - Gets paginated list of countries
- `GetCountryByIdQuery` - Gets country by ID
- `GetCountryByCodeQuery` - Gets country by code
- `GetActiveCountriesQuery` - Gets active countries only

**DTOs**:
- `CountryDto` - Complete country information
- `CountryLookupDto` - Minimal country information for dropdowns
- `CreateCountryDto` - Country creation request
- `UpdateCountryDto` - Country update request

**Validators**:
- `CreateCountryCommandValidator` - Validates country creation
- `UpdateCountryCommandValidator` - Validates country updates

**Mappers**:
- `CountryMappingProfile` - AutoMapper configuration for country entities

## Planned Implementation

### Common Components

#### Application Interfaces
```csharp
// Common/Interfaces/IApplicationService.cs
public interface IApplicationService
{
    Task<Result<TResponse>> ExecuteAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default);
}

// Common/Interfaces/IEmailService.cs
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendEmailTemplateAsync<T>(string to, string templateName, T model, CancellationToken cancellationToken = default);
}

// Common/Interfaces/IFileService.cs
public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}

// Common/Interfaces/INotificationService.cs
public interface INotificationService
{
    Task SendNotificationAsync(int userId, string title, string message, CancellationToken cancellationToken = default);
    Task SendBulkNotificationAsync(IEnumerable<int> userIds, string title, string message, CancellationToken cancellationToken = default);
}
```

#### Common Models
```csharp
// Common/Models/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string Error { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
    public static Result<T> Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors.ToList() };
}

// Common/Models/PagedResult.cs
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

// Common/Models/PagedRequest.cs
public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; } = string.Empty;
    public string SortBy { get; set; } = string.Empty;
    public bool SortDescending { get; set; } = false;
}
```

#### Application Exceptions
```csharp
// Common/Exceptions/ApplicationException.cs
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message) { }
    protected ApplicationException(string message, Exception innerException) : base(message, innerException) { }
}

// Common/Exceptions/ValidationException.cs
public class ValidationException : ApplicationException
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}

// Common/Exceptions/NotFoundException.cs
public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key '{key}' was not found") { }
}

// Common/Exceptions/BusinessRuleException.cs
public class BusinessRuleException : ApplicationException
{
    public BusinessRuleException(string message) : base(message) { }
}
```

### Feature-Based Organization

#### Inventory Features

##### Product Commands
```csharp
// Features/Inventory/Commands/CreateProduct/CreateProductCommand.cs
public record CreateProductCommand(
    string Code,
    string Name,
    string Description,
    string Category,
    decimal Price,
    decimal Cost,
    int StockQuantity,
    int MinimumStockLevel,
    string Unit
) : IRequest<Result<int>>;

// Features/Inventory/Commands/CreateProduct/CreateProductCommandHandler.cs
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateProductCommand> validator)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<int>.Failure(validationResult.Errors.Select(e => e.ErrorMessage));

        // Check business rules
        var existingProduct = await _productRepository.GetBySkuAsync(request.SKU, cancellationToken);
        if (existingProduct != null)
            return Result<int>.Failure($"Product with SKU '{request.SKU}' already exists");

        // Create domain entity
        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            Cost = request.Cost,
            StockQuantity = request.StockQuantity,
            MinimumStockLevel = request.MinimumStockLevel,
            Unit = request.Unit,
            IsActive = true
        };

        // Save to repository
        var createdProduct = await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(createdProduct.Id);
    }
}

// Features/Inventory/Commands/UpdateProduct/UpdateProductCommand.cs
public record UpdateProductCommand(
    int Id,
    string Code,
    string Name,
    string Description,
    string Category,
    decimal Price,
    decimal Cost,
    int MinimumStockLevel,
    string Unit,
    bool IsActive
) : IRequest<Result<bool>>;

// Features/Inventory/Commands/UpdateStock/UpdateStockCommand.cs
public record UpdateStockCommand(
    int ProductId,
    int Quantity,
    string Reason
) : IRequest<Result<bool>>;
```

##### Product Queries
```csharp
// Features/Inventory/Queries/GetProduct/GetProductQuery.cs
public record GetProductQuery(int Id) : IRequest<Result<ProductDto>>;

// Features/Inventory/Queries/GetProduct/GetProductQueryHandler.cs
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            return Result<ProductDto>.Failure($"Product with ID {request.Id} not found");

        var productDto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(productDto);
    }
}

// Features/Inventory/Queries/GetProducts/GetProductsQuery.cs
public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SearchTerm = "",
    string Category = "",
    bool? IsActive = null
) : IRequest<Result<PagedResult<ProductListDto>>>;

// Features/Inventory/Queries/GetLowStockProducts/GetLowStockProductsQuery.cs
public record GetLowStockProductsQuery() : IRequest<Result<IEnumerable<ProductDto>>>;
```

##### Product DTOs
```csharp
// Features/Inventory/DTOs/ProductDto.cs
public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLowStock => StockQuantity <= MinimumStockLevel;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string UpdatedByName { get; set; } = string.Empty;
}

// Features/Inventory/DTOs/ProductListDto.cs
public class ProductListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
}

// Features/Inventory/DTOs/CreateProductDto.cs
public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
}
```

### Application Services

#### ProductService
```csharp
// Services/IProductService.cs
public interface IProductService
{
    Task<Result<int>> CreateProductAsync(CreateProductDto productDto, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateProductAsync(int id, UpdateProductDto productDto, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> GetProductByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductListDto>>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateStockAsync(int productId, int quantity, string reason, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ProductDto>>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
}

// Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly IMediator _mediator;

    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<int>> CreateProductAsync(CreateProductDto productDto, CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            productDto.Code,
            productDto.Name,
            productDto.Description,
            productDto.Category,
            productDto.Price,
            productDto.Cost,
            productDto.StockQuantity,
            productDto.MinimumStockLevel,
            productDto.Unit);

        return await _mediator.Send(command, cancellationToken);
    }

    // Additional method implementations...
}
```

### Validation

#### Product Validators
```csharp
// Validators/CreateProductCommandValidator.cs
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level cannot be negative");
    }
}
```

### Mapping

#### AutoMapper Profiles
```csharp
// Mappers/ProductProfile.cs
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.StockQuantity <= src.MinimumStockLevel));

        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.StockQuantity <= src.MinimumStockLevel));

        CreateMap<CreateProductDto, CreateProductCommand>();
        CreateMap<UpdateProductDto, UpdateProductCommand>();
    }
}
```

### Cross-Cutting Behaviors

#### Logging Behavior
```csharp
// Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);
        
        var response = await next();
        
        _logger.LogInformation("Handled {RequestName}: {@Response}", requestName, response);
        
        return response;
    }
}
```

#### Validation Behavior
```csharp
// Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Any())
            throw new ValidationException(failures.Select(f => f.ErrorMessage));

        return await next();
    }
}
```

### Unit of Work Pattern

```csharp
// Interfaces/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

## Dependencies

### Required NuGet Packages
```xml
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.8.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

### Project References
```xml
<ProjectReference Include="..\GoldenFiberERP.Domain\GoldenFiberERP.Domain.csproj" />
```

## Benefits of This Architecture

### 1. Separation of Concerns
- Clear separation between use cases and domain logic
- Application services coordinate domain operations
- DTOs isolate external contracts from domain models

### 2. Testability
- Easy to unit test individual use cases
- Mock external dependencies through interfaces
- Validate business logic without external infrastructure

### 3. Flexibility
- Easy to add new use cases without modifying existing ones
- Pluggable behaviors for cross-cutting concerns
- Multiple presentation layers can use the same application layer

### 4. Maintainability
- Feature-based organization improves code navigation
- Consistent patterns across all use cases
- Clear dependency flow and boundaries

The Application layer serves as the orchestration hub that coordinates domain objects to fulfill specific business use cases while maintaining clean separation from infrastructure concerns.
