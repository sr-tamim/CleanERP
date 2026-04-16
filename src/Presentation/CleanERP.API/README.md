# CleanERP.API - Presentation Layer

This is the Presentation layer of the CleanERP system, built with ASP.NET Core Web API following Clean Architecture principles.

## Quick Start

```bash
# Build and run
dotnet run

# Access Swagger UI (opens automatically)
https://localhost:7266/swagger
```

## Project Structure

```
CleanERP.API/
├── Controllers/           # API controllers
├── Extensions/           # Swagger and service extensions
├── Middleware/          # Custom middleware
├── Properties/          # Launch settings
└── wwwroot/            # Static files (Swagger customizations)
```

## Key Features

- **Module-wise Swagger Documentation** - Separate docs per business module
- **Clean Architecture Integration** - Proper dependency injection and layer separation  
- **Global Exception Handling** - Consistent error responses
- **Health Monitoring** - Comprehensive health check endpoints

## Documentation

For detailed documentation, see:
- [Swagger Modules Implementation](../../docs/swagger/swagger-modules-implementation.md)
- [System Health Default Launch](../../docs/swagger/system-health-default-launch.md)
- [Clean Architecture Overview](../../docs/architecture/02-clean-architecture.md)

## Dependencies

### Project References
- `CleanERP.Application` - Business logic and use cases
- `CleanERP.Infrastructure` - Infrastructure services
- `CleanERP.Persistence` - Database access

### Key NuGet Packages
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation
- `Microsoft.EntityFrameworkCore.Design` - EF Core design-time tools
- `FluentValidation.AspNetCore` - Model validation

## Development Notes

- **Swagger UI**: Auto-opens to System Health module by default
- **CORS**: Configured for development (all origins allowed)
- **Exception Handling**: Global middleware handles all exceptions
- **Module-wise Documentation**: Each business domain has separate Swagger docs

## Current Controllers

- `ProductsController` → Inventory Management module
- `HealthController` → System & Health module  
- `AuthController` → Authentication & Authorization module
- `SalesController` → Sales Management module

New controllers are automatically categorized based on naming conventions.
