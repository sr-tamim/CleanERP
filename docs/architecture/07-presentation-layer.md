# Presentation Layer

## Overview

The Presentation layer is the outermost layer of the application that handles user interactions and external communication. In GoldenFiberERP, this is implemented as a Web API that provides RESTful endpoints for client applications to interact with the business logic.

## Design Principles

### 1. RESTful Design
- Standard HTTP methods (GET, POST, PUT, DELETE)
- Resource-based URLs
- Proper HTTP status codes
- Consistent response formats
- Stateless communication

### 2. Separation of Concerns
- Controllers handle HTTP concerns only
- Business logic delegated to Application layer
- Input validation and model binding
- Response formatting and serialization

### 3. Security First
- Authentication and authorization
- Input sanitization and validation
- Rate limiting and throttling
- CORS configuration
- Security headers

## Current Implementation

### Project Structure
**Location**: `src/Presentation/GoldenFiberERP.API`

**Current Files**:
```
GoldenFiberERP.API/
├── GoldenFiberERP.API.csproj      # Project configuration
├── Program.cs                      # Application entry point
├── appsettings.json               # Production configuration
├── appsettings.Development.json   # Development configuration
├── Dockerfile                     # Container definition
├── GoldenFiberERP.API.http        # HTTP client test file
├── WeatherForecast.cs             # Sample model (to be removed)
├── Controllers/                   # API controllers
│   ├── WeatherForecastController.cs # Sample controller (to be removed)
│   └── Settings/                  # Settings module controllers
│       └── CountriesController.cs # Country CRUD operations
├── Extensions/                    # API extensions and configuration
│   ├── SwaggerExtensions.cs       # Swagger configuration
│   └── SwaggerOperationFilter.cs  # Swagger operation filters
└── Properties/
    └── launchSettings.json        # Launch profiles
```

### Current Configuration

#### Program.cs
The application entry point is configured with:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddApplication(); // From Application layer
builder.Services.AddInfrastructure(builder.Configuration); // From Infrastructure layer
builder.Services.AddPersistence(builder.Configuration); // From Persistence layer

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Status**: Production-ready configuration with proper dependency injection and Swagger documentation.

### Implemented Controllers

#### CountriesController
**File**: `Controllers/Settings/CountriesController.cs`

```csharp
[ApiController]
[Route("api/settings/[controller]")]
[Tags("Settings - Countries")]
public class CountriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CountriesController> _logger;

    // GET /api/settings/countries
    [HttpGet]
    public async Task<IActionResult> GetCountries([FromQuery] GetCountriesQuery query)

    // GET /api/settings/countries/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCountry(int id)

    // POST /api/settings/countries
    [HttpPost]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDto dto)

    // PUT /api/settings/countries/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDto dto)

    // DELETE /api/settings/countries/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)

    // PATCH /api/settings/countries/{id}/activate
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> ActivateCountry(int id)

    // PATCH /api/settings/countries/{id}/deactivate
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateCountry(int id)
}
```

**Features**:
- **Complete CRUD Operations**: Create, Read, Update, Delete
- **RESTful Design**: Follows REST conventions
- **Proper HTTP Status Codes**: 200, 201, 400, 404, 500
- **Error Handling**: Comprehensive exception handling
- **Logging**: Structured logging for operations
- **Swagger Documentation**: Automatic API documentation
- **Validation**: Input validation using FluentValidation
- **CQRS Pattern**: Uses MediatR for command/query separation

### Swagger Configuration

#### SwaggerExtensions
**File**: `Extensions/SwaggerExtensions.cs`

Provides modular organization of API endpoints:
- **Settings Module**: Countries and other settings
- **Inventory Module**: Product management (planned)
- **Manufacturing Module**: Production operations (planned)
- **Sales Module**: Orders and customers (planned)

#### SwaggerOperationFilter
**File**: `Extensions/SwaggerOperationFilter.cs`

Automatically categorizes controllers into logical modules for better API documentation organization.

## Planned Implementation

### Enhanced Project Structure
```
GoldenFiberERP.API/
├── Controllers/                   # API controllers
│   ├── Common/                    # Base controllers
│   ├── Inventory/                 # Inventory management
│   ├── Manufacturing/             # Manufacturing operations
│   ├── Sales/                     # Sales and orders
│   ├── Financial/                 # Financial operations
│   └── Admin/                     # Administrative functions
├── Models/                        # API models
│   ├── Requests/                  # Request DTOs
│   ├── Responses/                 # Response DTOs
│   └── Common/                    # Common models
├── Middleware/                    # Custom middleware
│   ├── ErrorHandlingMiddleware.cs
│   ├── LoggingMiddleware.cs
│   ├── RateLimitingMiddleware.cs
│   └── SecurityHeadersMiddleware.cs
├── Filters/                       # Action filters
│   ├── ValidationFilter.cs
│   ├── AuthorizationFilter.cs
│   └── CacheFilter.cs
├── Configuration/                 # Startup configuration
│   ├── ServicesConfiguration.cs
│   ├── MiddlewareConfiguration.cs
│   └── SwaggerConfiguration.cs
├── Extensions/                    # Extension methods
│   └── ServiceCollectionExtensions.cs
├── Validators/                    # Request validators
│   └── FluentValidation/
└── Documentation/                 # API documentation
```

### Enhanced Program.cs
```csharp
using GoldenFiberERP.API.Configuration;
using GoldenFiberERP.Application;
using GoldenFiberERP.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add API-specific services
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

// Development configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
```

## Controller Implementation

### Base Controller
```csharp
// Controllers/Common/BaseApiController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Data != null ? Ok(result.Data) : NotFound();
        }

        return BadRequest(new { errors = result.Errors.Any() ? result.Errors : new[] { result.Error } });
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return BadRequest(new { errors = result.Errors.Any() ? result.Errors : new[] { result.Error } });
    }

    protected ActionResult<PagedResponse<T>> HandlePagedResult<T>(Result<PagedResult<T>> result)
    {
        if (result.IsSuccess && result.Data != null)
        {
            var response = new PagedResponse<T>
            {
                Data = result.Data.Items,
                PageNumber = result.Data.PageNumber,
                PageSize = result.Data.PageSize,
                TotalCount = result.Data.TotalCount,
                TotalPages = result.Data.TotalPages,
                HasNextPage = result.Data.HasNextPage,
                HasPreviousPage = result.Data.HasPreviousPage
            };

            return Ok(response);
        }

        return BadRequest(new { errors = result.Errors.Any() ? result.Errors : new[] { result.Error } });
    }
}
```

### Product Controller
```csharp
// Controllers/Inventory/ProductsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// Get all products with optional filtering and pagination
    /// </summary>
    /// <param name="request">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ProductListDto>>> GetProducts([FromQuery] GetProductsRequest request)
    {
        var query = new GetProductsQuery(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm ?? string.Empty,
            request.Category ?? string.Empty,
            request.IsActive);

        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var query = new GetProductQuery(id);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a product by its code
    /// </summary>
    /// <param name="code">Product code</param>
    /// <returns>Product details</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> GetProductByCode(string code)
    {
        var query = new GetProductByCodeQuery(code);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>Created product ID</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Code,
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.Cost,
            request.StockQuantity,
            request.MinimumStockLevel,
            request.Unit);

        var result = await Mediator.Send(command);
        
        if (result.IsSuccess)
        {
            var response = new CreateProductResponse { Id = result.Data };
            return CreatedAtAction(nameof(GetProduct), new { id = result.Data }, response);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update data</param>
    /// <returns>Success indicator</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var command = new UpdateProductCommand(
            id,
            request.Code,
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.Cost,
            request.MinimumStockLevel,
            request.Unit,
            request.IsActive);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success indicator</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Stock update data</param>
    /// <returns>Success indicator</returns>
    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
    {
        var command = new UpdateStockCommand(id, request.Quantity, request.Reason);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get products with low stock levels
    /// </summary>
    /// <returns>List of low stock products</returns>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
    {
        var query = new GetLowStockProductsQuery();
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}
```

## Request/Response Models

### Product Request Models
```csharp
// Models/Requests/Products/GetProductsRequest.cs
public class GetProductsRequest : PagedRequest
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

// Models/Requests/Products/CreateProductRequest.cs
public class CreateProductRequest
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Cost cannot be negative")]
    public decimal Cost { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative")]
    public int MinimumStockLevel { get; set; }

    [Required]
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;
}

// Models/Requests/Products/UpdateProductRequest.cs
public class UpdateProductRequest
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Cost { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int MinimumStockLevel { get; set; }

    [Required]
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

// Models/Requests/Products/UpdateStockRequest.cs
public class UpdateStockRequest
{
    [Required]
    public int Quantity { get; set; }

    [Required]
    [StringLength(200)]
    public string Reason { get; set; } = string.Empty;
}
```

### Response Models
```csharp
// Models/Responses/Common/PagedResponse.cs
public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

// Models/Responses/Common/ApiResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Models/Responses/Products/CreateProductResponse.cs
public class CreateProductResponse
{
    public int Id { get; set; }
}
```

## Middleware Implementation

### Error Handling Middleware
```csharp
// Middleware/ErrorHandlingMiddleware.cs
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var apiResponse = new ApiResponse<object>
        {
            Success = false,
            Message = "An error occurred while processing your request",
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Errors = validationEx.Errors;
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = StatusCodes.Status404NotFound;
                apiResponse.Message = notFoundEx.Message;
                break;

            case BusinessRuleException businessRuleEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Message = businessRuleEx.Message;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                apiResponse.Message = "Unauthorized access";
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                apiResponse.Message = "An internal server error occurred";
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}
```

### Security Headers Middleware
```csharp
// Middleware/SecurityHeadersMiddleware.cs
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");

        await _next(context);
    }
}
```

## Filters

### Validation Filter
```csharp
// Filters/ValidationFilter.cs
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage);

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Post-action logic if needed
    }
}
```

## Authentication & Authorization

### JWT Configuration
```csharp
// Configuration/AuthenticationConfiguration.cs
public static class AuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

        return services;
    }
}
```

## API Documentation

### Swagger Configuration
```csharp
// Configuration/SwaggerConfiguration.cs
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "GoldenFiberERP API",
                Description = "Enterprise Resource Planning System for Fiber and Textile Industries",
                Contact = new OpenApiContact
                {
                    Name = "GoldenFiberERP Support",
                    Email = "support@goldenfibererp.com"
                }
            });

            // Add JWT authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
```

## Required Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="5.1.0" />
<PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="8.0.0" />
```

### Project References
```xml
<ProjectReference Include="..\..\Core\GoldenFiberERP.Application\GoldenFiberERP.Application.csproj" />
<ProjectReference Include="..\..\Infrastructure\GoldenFiberERP.Infrastructure\GoldenFiberERP.Infrastructure.csproj" />
```

## Benefits

### 1. Clean API Design
- RESTful endpoints with consistent patterns
- Proper HTTP status codes and error handling
- Clear request/response models
- Comprehensive API documentation

### 2. Security
- JWT-based authentication
- Role-based authorization
- Security headers and CORS configuration
- Input validation and sanitization

### 3. Maintainability
- Separation of concerns between layers
- Consistent error handling and logging
- Modular middleware pipeline
- Testable controller design

### 4. Developer Experience
- Swagger documentation for API exploration
- Consistent response formats
- Clear validation error messages
- Comprehensive logging for debugging

The Presentation layer provides a robust, secure, and well-documented API interface that effectively exposes the business functionality while maintaining proper separation of concerns and following REST best practices.
