# GoldenFiberERP.Persistence Layer

This is the Persistence layer of the GoldenFiberERP system, responsible for data access and database operations using Entity Framework Core with PostgreSQL.

## Structure

### Contexts
- **ApplicationDbContext**: Main EF Core DbContext implementing `IUnitOfWork`
  - Automatic audit field population (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
  - Configuration loading from assembly
  - PostgreSQL optimized

### Configurations
- **ProductConfiguration**: Entity configuration for Product entity
  - Column mappings and constraints
  - Indexes for performance
  - Relationships and foreign keys

### Repositories
- **ProductRepository**: Implementation of `IProductRepository`
  - CRUD operations
  - Business-specific queries (low stock, search by name, etc.)
  - Async operations with cancellation token support
- **CountryRepository**: Implementation of `ICountryRepository`
  - Settings-specific queries and paging
  - Specification-based query helpers

### Services
- **HealthCheckService**: Implementation of `IHealthCheckService`
  - Database connectivity checks
  - PostgreSQL-specific diagnostics
  - Schema validation for manual management

### Seeders
- **ApplicationDbContextSeed**: Database seeding with sample data
  - Initial product data for textile/fiber industry
  - Ensures database creation
  - Safe seeding (only if data doesn't exist)

## Key Features

### Entity Framework Core
- Code-first approach with migrations
- PostgreSQL provider for production database
- Automatic audit trail implementation
- Query optimization with proper indexing

### Repository Pattern
- Abstraction over data access
- Business-focused query methods
- Testable and mockable interfaces
- Consistent error handling
- Shared base repository for common CRUD and specification support
 
### Health Checks
- Database health checks live in Persistence to keep DB access in one place

### Database Seeding
- Sample data for development and testing
- Industry-specific examples (textile/fiber products)
- Safe initialization procedures

## Dependencies

### NuGet Packages
- `Microsoft.EntityFrameworkCore` - Core EF functionality
- `Microsoft.EntityFrameworkCore.Design` - Design-time tools
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `Microsoft.Extensions.Configuration.Abstractions` - Configuration support
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI support

## Configuration

### Connection String
Add to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GoldenFiberERP;Username=postgres;Password=your_password"
  }
}
```

### Registration in Startup/Program.cs
```csharp
services.AddPersistence(configuration);
```

## Migration Commands

### Add Migration
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure/GoldenFiberERP.Persistence --startup-project src/Presentation/GoldenFiberERP.API
```

### Update Database
```bash
dotnet ef database update --project src/Infrastructure/GoldenFiberERP.Persistence --startup-project src/Presentation/GoldenFiberERP.API
```

## Sample Data

The seeder includes sample textile/fiber industry products:
- Cotton Fabric - White
- Polyester Thread - Black  
- Silk Fabric - Blue
- Denim Fabric - Indigo
- Wool Yarn - Gray

## Notes

- Designed for PostgreSQL but can be easily switched to other providers
- Automatic audit fields are populated on save
- Repository pattern provides clean separation from EF Core
- All operations are async for better performance
- Proper indexing for common query patterns
