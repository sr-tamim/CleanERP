# Domain Layer

## Overview

The Domain layer represents the core business logic of the GoldenFiberERP system. It contains the business entities, domain services, value objects, and business rules that are independent of any external concerns such as databases, frameworks, or user interfaces.

## Design Principles

### 1. Domain-Driven Design (DDD)
The domain layer follows DDD principles:
- **Ubiquitous Language**: Uses business terminology consistently
- **Bounded Contexts**: Clear boundaries around business concepts
- **Rich Domain Models**: Entities contain behavior, not just data
- **Business Rules**: Encapsulated within domain objects

### 2. Dependency Independence
- No dependencies on external frameworks
- Framework-agnostic design
- Pure business logic implementation
- Interface-based contracts for external dependencies

## Current Implementation

### Base Entities

#### BaseEntity
**File**: `Entities/Common/BaseEntity.cs`

```csharp
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**Purpose**:
- Provides common properties for all entities
- Ensures consistent identity and auditing
- Uses UTC timestamps for consistency across time zones

**Key Features**:
- **Primary Key**: Integer-based identity
- **Creation Tracking**: Automatic timestamp on creation
- **Update Tracking**: Timestamp for last modification
- **Abstract Class**: Cannot be instantiated directly

#### AuditableEntity
**File**: `Entities/Common/AuditableEntity.cs`

```csharp
public abstract class AuditableEntity : BaseEntity, ISoftDeleteEntity
{
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
```

**Purpose**:
- Extends BaseEntity with user tracking
- Implements soft delete functionality
- Provides comprehensive audit trail

**Key Features**:
- **User Tracking**: CreatedBy, UpdatedBy, DeletedBy
- **Soft Delete**: Logical deletion without physical removal
- **Audit Trail**: Complete change history
- **Nullable References**: Supports anonymous operations

#### ISoftDeleteEntity Interface
**File**: `Entities/Common/ISoftDeleteEntity.cs`

```csharp
public interface ISoftDeleteEntity
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedBy { get; set; }
}
```

**Purpose**:
- Defines contract for soft delete functionality
- Ensures consistent implementation across entities
- Supports data retention policies

### Domain Entities

#### Product Entity
**File**: `Entities/Inventory/Product.cs`

```csharp
public class Product : AuditableEntity
{
    // Implementation to be expanded
}
```

**Current Status**: Basic structure established
**Planned Properties**:
```csharp
public class Product : AuditableEntity
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
    public bool IsActive { get; set; } = true;
    
    // Business Methods
    public void UpdateStock(int quantity);
    public bool IsInStock(int requiredQuantity);
    public bool IsLowStock();
    public void Activate();
    public void Deactivate();
}
```

### Domain Enumerations

#### UserRole Enum
**File**: `Enums/UserRole.cs`

```csharp
public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    Manager = 3,
    Employee = 4
}
```

**Purpose**:
- Defines user roles in the system
- Hierarchical permission structure
- Type-safe role assignment

**Planned Enumerations**:
- **ProductStatus**: Active, Inactive, Discontinued
- **StockMovementType**: In, Out, Transfer, Adjustment
- **OrderStatus**: Pending, Confirmed, Shipped, Delivered, Cancelled
- **PaymentStatus**: Pending, Paid, Overdue, Cancelled

### Domain Exceptions

#### InsufficientStockException
**File**: `Exceptions/InsufficientStockException.cs`

```csharp
public class InsufficientStockException(string message) : Exception(message)
{
}
```

**Purpose**:
- Domain-specific exception for stock management
- Clear business rule violation indication
- Consistent error handling across the domain

**Planned Exceptions**:
- **ProductNotFoundException**
- **InvalidPriceException**
- **DuplicateProductCodeException**
- **OrderCannotBeCancelledException**

### Repository Interfaces

#### IBaseRepository Interface
**File**: `Interfaces/Repositories/Common/IBaseRepository.cs`

```csharp
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
```

**Purpose**:
- Defines standard CRUD operations
- Provides consistent data access interface
- Supports async operations with cancellation

**Key Features**:
- **Generic Implementation**: Works with any BaseEntity
- **Async/Await Pattern**: Non-blocking operations
- **Cancellation Support**: Cooperative cancellation
- **Null Safety**: Nullable return types where appropriate

#### IProductRepository Interface
**File**: `Interfaces/Repositories/Inventory/IProductRepository.cs`

```csharp
public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetByCode(string productCode, CancellationToken cancellationToken = default);
}
```

**Purpose**:
- Extends base repository with product-specific operations
- Business-specific query methods
- Domain-driven data access contracts

**Planned Methods**:
```csharp
public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetByCode(string productCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategory(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProducts(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchByName(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> IsCodeUnique(string code, int? excludeId = null, CancellationToken cancellationToken = default);
}
```

## Planned Domain Entities

### Customer Entity
```csharp
public class Customer : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public CustomerType Type { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Order Entity
```csharp
public class Order : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
    
    // Business Methods
    public void AddItem(Product product, int quantity, decimal unitPrice);
    public void RemoveItem(int productId);
    public void CalculateTotal();
    public bool CanBeCancelled();
    public void Cancel();
}
```

### Supplier Entity
```csharp
public class Supplier : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public PaymentTerms PaymentTerms { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## Value Objects

### Address Value Object
```csharp
public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    
    public string FullAddress => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
```

### Money Value Object
```csharp
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    
    public static Money Zero => new(0, "USD");
    
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        
        return new Money(Amount + other.Amount, Currency);
    }
}
```

## Domain Services

### StockManagementService
```csharp
public interface IStockManagementService
{
    Task<bool> ReserveStock(int productId, int quantity);
    Task ReleaseStock(int productId, int quantity);
    Task AdjustStock(int productId, int quantity, string reason);
    Task<IEnumerable<Product>> GetLowStockProducts();
}
```

### PricingService
```csharp
public interface IPricingService
{
    decimal CalculatePrice(Product product, Customer customer, int quantity);
    decimal CalculateDiscount(Customer customer, decimal amount);
    bool ValidatePrice(decimal price, Product product);
}
```

## Domain Events

### ProductStockChanged
```csharp
public record ProductStockChanged(
    int ProductId,
    int PreviousQuantity,
    int NewQuantity,
    string Reason,
    DateTime OccurredAt
) : IDomainEvent;
```

### OrderCreated
```csharp
public record OrderCreated(
    int OrderId,
    int CustomerId,
    decimal TotalAmount,
    DateTime CreatedAt
) : IDomainEvent;
```

## Business Rules Implementation

### Entity Validation
- **Data Annotations**: Basic validation attributes
- **Business Logic**: Custom validation methods
- **Invariant Enforcement**: Constructor and method validation

### Business Rule Examples
```csharp
public class Product : AuditableEntity
{
    private decimal _price;
    
    public decimal Price
    {
        get => _price;
        set
        {
            if (value < 0)
                throw new InvalidPriceException("Price cannot be negative");
            _price = value;
        }
    }
    
    public void UpdateStock(int quantity)
    {
        if (StockQuantity + quantity < 0)
            throw new InsufficientStockException($"Insufficient stock for product {Code}");
        
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

## Future Enhancements

### 1. Domain Events
- Event sourcing implementation
- Domain event publishing
- Event handlers for business processes

### 2. Aggregates
- Aggregate root implementation
- Transaction boundaries
- Consistency enforcement

### 3. Specifications
- Business rule specifications
- Query specifications
- Validation specifications

### 4. Value Objects
- Complex value objects
- Immutable design
- Equality implementation

The Domain layer serves as the foundation of the GoldenFiberERP system, providing a rich, expressive model of the business domain that remains stable as external technologies and frameworks evolve.
