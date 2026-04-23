# CleanERP.Application Layer

This is the Application layer of the CleanERP system, following Clean Architecture principles. This layer contains the application's use cases and business logic orchestration.

## Structure

### Common
- **Behaviors/**: Cross-cutting concerns like validation behaviors for MediatR pipeline
- **Exceptions/**: Application-specific exceptions
- **Interfaces/**: Contracts for external dependencies (repositories, services, etc.)
- **Mappers/**: AutoMapper profiles for object mapping
- **Models/**: Common models like Result patterns

### Features
Organized by business domains, following the CQRS pattern:

#### Inventory
- **Commands/**: Write operations (Create, Update, Delete)
- **Queries/**: Read operations (Get, List)
- **DTOs/**: Data Transfer Objects for API contracts
- **Validators/**: FluentValidation validators for commands/queries

## Key Patterns

### CQRS (Command Query Responsibility Segregation)
- Commands: Modify state, return `Result` or `Result<T>`
- Queries: Read data, return `Result<T>` with data

### MediatR Pipeline
- All requests go through MediatR
- Automatic validation via `ValidationBehavior`
- Separation of concerns

### Result Pattern
- Consistent return types across the application
- Success/failure states with error messages
- Type-safe error handling

## Dependencies

### NuGet Packages (to be added)
- `MediatR` - CQRS and mediator pattern
- `AutoMapper` - Object mapping
- `FluentValidation` - Input validation

## Example Usage

### Creating a Product
```csharp
var command = new CreateProductCommand
{
    Name = "Sample Product",
    Price = 100.00m,
    StockQuantity = 50
};

var result = await mediator.Send(command);
if (result.Succeeded)
{
    var productId = result.Data;
    // Handle success
}
```

### Querying Products
```csharp
var query = new GetProductsQuery();
var result = await mediator.Send(query);
if (result.Succeeded)
{
    var products = result.Data;
    // Handle products
}
```

## Notes

This is a scaffold/example implementation. The actual business logic should be implemented based on specific requirements. The structure provides a foundation for:

- Maintainable code organization
- Testable business logic
- Consistent error handling
- Scalable architecture
