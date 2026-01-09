# Design Patterns

## Overview

The GoldenFiberERP system implements several well-established design patterns to ensure maintainability, testability, and scalability. This document outlines the key patterns used throughout the application and their implementation details.

## Architectural Patterns

### 1. Clean Architecture Pattern
**Purpose**: Separates business logic from external concerns and dependencies.

**Implementation**:
- **Domain Layer**: Contains business entities and rules
- **Application Layer**: Orchestrates business operations
- **Infrastructure Layer**: Implements external dependencies
- **Presentation Layer**: Handles user interface and API concerns

**Benefits**:
- Framework independence
- Testable business logic
- Flexible and maintainable codebase
- Clear separation of responsibilities

### 2. Repository Pattern
**Purpose**: Encapsulates data access logic and provides a uniform interface for data operations.

**Implementation**:
```csharp
// Domain layer interface
public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
}

// Infrastructure layer implementation
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }
}
```

**Benefits**:
- Centralized data access logic
- Easy unit testing with mock repositories
- Consistent query patterns
- Database technology independence

### 3. Unit of Work Pattern
**Purpose**: Maintains a list of objects affected by business transactions and coordinates writing out changes.

**Implementation**:
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

// DbContext implements IUnitOfWork
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle audit fields, domain events, etc.
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

**Benefits**:
- Ensures data consistency
- Manages transaction boundaries
- Coordinates multiple repository operations
- Improves performance by batching database operations

## Behavioral Patterns

### 4. CQRS (Command Query Responsibility Segregation)
**Purpose**: Separates read and write operations to optimize performance and maintainability.

**Implementation**:
```csharp
// Command (Write operation)
public record CreateProductCommand(
    string Code,
    string Name,
    decimal Price
) : IRequest<Result<int>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Handle product creation
    }
}

// Query (Read operation)
public record GetProductQuery(int Id) : IRequest<Result<ProductDto>>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        // Handle product retrieval
    }
}
```

**Benefits**:
- Optimized read and write models
- Improved performance
- Better scalability
- Clear separation of concerns

### 5. Mediator Pattern
**Purpose**: Reduces coupling between components by centralizing request handling.

**Implementation**:
```csharp
// Using MediatR library
[ApiController]
public class ProductsController : BaseApiController
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(request.Code, request.Name, request.Price);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
```

**Benefits**:
- Loose coupling between controllers and business logic
- Centralized request processing
- Easy to add cross-cutting concerns (logging, validation)
- Simplified controller code

### 6. Strategy Pattern
**Purpose**: Allows selecting algorithms at runtime based on specific conditions.

**Implementation**:
```csharp
// Pricing strategies
public interface IPricingStrategy
{
    decimal CalculatePrice(Product product, Customer customer, int quantity);
}

public class StandardPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product, Customer customer, int quantity)
    {
        return product.Price * quantity;
    }
}

public class VipCustomerPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product, Customer customer, int quantity)
    {
        var discount = quantity > 100 ? 0.15m : 0.10m;
        return product.Price * quantity * (1 - discount);
    }
}

// Context
public class PricingService : IPricingService
{
    public decimal CalculatePrice(Product product, Customer customer, int quantity)
    {
        IPricingStrategy strategy = customer.Type switch
        {
            CustomerType.Vip => new VipCustomerPricingStrategy(),
            CustomerType.Corporate => new CorporatePricingStrategy(),
            _ => new StandardPricingStrategy()
        };

        return strategy.CalculatePrice(product, customer, quantity);
    }
}
```

**Benefits**:
- Flexible algorithm selection
- Easy to add new strategies
- Improved testability
- Adherence to Open/Closed Principle

## Creational Patterns

### 7. Factory Pattern
**Purpose**: Creates objects without specifying their exact classes.

**Implementation**:
```csharp
// Factory for creating domain events
public interface IDomainEventFactory
{
    IDomainEvent CreateProductStockChanged(int productId, int oldQuantity, int newQuantity, string reason);
    IDomainEvent CreateOrderCreated(int orderId, int customerId, decimal totalAmount);
}

public class DomainEventFactory : IDomainEventFactory
{
    public IDomainEvent CreateProductStockChanged(int productId, int oldQuantity, int newQuantity, string reason)
    {
        return new ProductStockChangedEvent(productId, oldQuantity, newQuantity, reason, DateTime.UtcNow);
    }

    public IDomainEvent CreateOrderCreated(int orderId, int customerId, decimal totalAmount)
    {
        return new OrderCreatedEvent(orderId, customerId, totalAmount, DateTime.UtcNow);
    }
}
```

**Benefits**:
- Encapsulates object creation logic
- Provides flexibility in object creation
- Supports the Single Responsibility Principle
- Easy to extend with new object types

### 8. Builder Pattern
**Purpose**: Constructs complex objects step by step.

**Implementation**:
```csharp
// Query builder for complex search criteria
public class ProductQueryBuilder
{
    private readonly ProductQuery _query = new();

    public ProductQueryBuilder WithCategory(string category)
    {
        _query.Category = category;
        return this;
    }

    public ProductQueryBuilder WithPriceRange(decimal minPrice, decimal maxPrice)
    {
        _query.MinPrice = minPrice;
        _query.MaxPrice = maxPrice;
        return this;
    }

    public ProductQueryBuilder WithStockStatus(StockStatus status)
    {
        _query.StockStatus = status;
        return this;
    }

    public ProductQueryBuilder WithPaging(int pageNumber, int pageSize)
    {
        _query.PageNumber = pageNumber;
        _query.PageSize = pageSize;
        return this;
    }

    public ProductQuery Build() => _query;
}

// Usage
var query = new ProductQueryBuilder()
    .WithCategory("Electronics")
    .WithPriceRange(100, 1000)
    .WithStockStatus(StockStatus.InStock)
    .WithPaging(1, 20)
    .Build();
```

**Benefits**:
- Simplifies complex object construction
- Provides fluent interface
- Improves code readability
- Allows step-by-step object building

## Structural Patterns

### 9. Adapter Pattern
**Purpose**: Allows incompatible interfaces to work together.

**Implementation**:
```csharp
// External payment service with different interface
public class ExternalPaymentService
{
    public PaymentResult ProcessTransaction(TransactionData data) { /* Implementation */ }
}

// Our domain interface
public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}

// Adapter to bridge the gap
public class ExternalPaymentServiceAdapter : IPaymentService
{
    private readonly ExternalPaymentService _externalService;

    public ExternalPaymentServiceAdapter(ExternalPaymentService externalService)
    {
        _externalService = externalService;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Convert our domain model to external service format
        var transactionData = new TransactionData
        {
            Amount = request.Amount,
            Currency = request.Currency,
            // Map other properties
        };

        // Call external service
        var result = _externalService.ProcessTransaction(transactionData);
        
        // Convert result back to our domain model
        return new PaymentResult
        {
            IsSuccess = result.Success,
            TransactionId = result.Id,
            // Map other properties
        };
    }
}
```

**Benefits**:
- Integrates third-party services seamlessly
- Maintains clean domain interfaces
- Isolates external dependencies
- Easy to switch implementations

### 10. Decorator Pattern
**Purpose**: Adds new functionality to objects dynamically without altering their structure.

**Implementation**:
```csharp
// Base email service
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

// Basic implementation
public class BasicEmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Basic email sending logic
    }
}

// Logging decorator
public class LoggingEmailServiceDecorator : IEmailService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<LoggingEmailServiceDecorator> _logger;

    public LoggingEmailServiceDecorator(IEmailService emailService, ILogger<LoggingEmailServiceDecorator> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
        
        try
        {
            await _emailService.SendEmailAsync(to, subject, body, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}

// Rate limiting decorator
public class RateLimitingEmailServiceDecorator : IEmailService
{
    private readonly IEmailService _emailService;
    private readonly IRateLimiter _rateLimiter;

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await _rateLimiter.WaitAsync(cancellationToken);
        await _emailService.SendEmailAsync(to, subject, body, cancellationToken);
    }
}
```

**Benefits**:
- Adds functionality without changing existing code
- Supports composition over inheritance
- Easy to combine multiple decorators
- Maintains single responsibility principle

## Domain-Driven Design Patterns

### 11. Aggregate Pattern
**Purpose**: Groups related entities and value objects to maintain consistency.

**Implementation**:
```csharp
// Order aggregate root
public class Order : AuditableEntity
{
    private readonly List<OrderItem> _orderItems = new();

    public string OrderNumber { get; private set; } = string.Empty;
    public int CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // Business methods that maintain aggregate consistency
    public void AddItem(Product product, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(product.Id, quantity, unitPrice);
            _orderItems.Add(orderItem);
        }

        RecalculateTotal();
    }

    public void RemoveItem(int productId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var item = _orderItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _orderItems.Remove(item);
            RecalculateTotal();
        }
    }

    private void RecalculateTotal()
    {
        TotalAmount = _orderItems.Sum(i => i.LineTotal);
    }
}

// Child entity
public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive");
        
        Quantity = newQuantity;
    }
}
```

**Benefits**:
- Ensures business rule consistency
- Provides clear transaction boundaries
- Encapsulates complex business logic
- Improves data integrity

### 12. Specification Pattern
**Purpose**: Encapsulates business rules and query logic in reusable components.

**Implementation**:
```csharp
// Base specification
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
}

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; private set; } = null!;
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
}

// Concrete specifications
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        AddCriteria(p => p.IsActive && !p.IsDeleted);
        AddOrderBy(p => p.Name);
    }
}

public class LowStockProductsSpecification : BaseSpecification<Product>
{
    public LowStockProductsSpecification()
    {
        AddCriteria(p => p.StockQuantity <= p.MinimumStockLevel && p.IsActive);
        AddOrderBy(p => p.StockQuantity);
    }
}

public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(string category)
    {
        AddCriteria(p => p.Category == category && p.IsActive);
        AddOrderBy(p => p.Name);
    }
}
```

**Benefits**:
- Reusable business rules
- Composable query logic
- Improved testability
- Clear expression of business concepts

## Cross-Cutting Patterns

### 13. Chain of Responsibility Pattern
**Purpose**: Passes requests along a chain of handlers until one handles it.

**Implementation**:
```csharp
// Pipeline behaviors in MediatR
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(request, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Any())
                throw new ValidationException(failures.Select(f => f.ErrorMessage));
        }

        return await next();
    }
}

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestName}: {@Request}", typeof(TRequest).Name, request);
        
        var response = await next();
        
        _logger.LogInformation("Handled {RequestName}: {@Response}", typeof(TRequest).Name, response);
        
        return response;
    }
}
```

**Benefits**:
- Flexible request processing pipeline
- Easy to add/remove behaviors
- Separation of cross-cutting concerns
- Consistent handling across all requests

## Benefits of Using These Patterns

### 1. **Maintainability**
- Clear separation of concerns
- Consistent code organization
- Easy to understand and modify

### 2. **Testability**
- Isolated components
- Mock-friendly interfaces
- Clear dependencies

### 3. **Flexibility**
- Easy to extend and modify
- Support for changing requirements
- Pluggable components

### 4. **Scalability**
- Efficient resource utilization
- Optimized read/write operations
- Flexible architecture

### 5. **Code Quality**
- Reduced duplication
- Improved readability
- Better error handling

### 6. **Team Collaboration**
- Consistent patterns across the codebase
- Clear architectural boundaries
- Shared understanding of code organization

These design patterns work together to create a robust, maintainable, and scalable enterprise application that can evolve with changing business requirements while maintaining code quality and developer productivity.
