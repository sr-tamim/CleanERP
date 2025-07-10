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

#### Country Entity
**File**: `Entities/Settings/Country.cs`

```csharp
public class Country : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Code3 { get; private set; } = string.Empty;
    public string NumericCode { get; private set; } = string.Empty;
    public string PhoneCode { get; private set; } = string.Empty;
    public string Capital { get; private set; } = string.Empty;
    public string CurrencyCode { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Country() { } // EF Constructor

    // Factory method for creating new countries
    public static Country Create(
        string name,
        string code,
        string code3,
        string numericCode,
        string phoneCode,
        string capital,
        string currencyCode,
        string region,
        int displayOrder,
        int createdBy)
    {
        var country = new Country
        {
            Name = name,
            Code = code,
            Code3 = code3,
            NumericCode = numericCode,
            PhoneCode = phoneCode,
            Capital = capital,
            CurrencyCode = currencyCode,
            Region = region,
            DisplayOrder = displayOrder,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Add domain event
        country.AddDomainEvent(new CountryCreatedEvent(country));
        return country;
    }

    // Business logic methods
    public void UpdateDetails(string name, string capital, string currencyCode, string region, int displayOrder)
    {
        Name = name;
        Capital = capital;
        CurrencyCode = currencyCode;
        Region = region;
        DisplayOrder = displayOrder;
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

**Current Status**: Fully implemented with domain events and business logic

### Domain Events

#### Country Events
**File**: `Events/Settings/CountryEvents.cs`

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

**Purpose**:
- Notify other parts of the system when country-related changes occur
- Enable loose coupling between domain operations
- Support event-driven architecture patterns
- Maintain audit trails and business process workflows

**Key Features**:
- **Record Types**: Immutable event definitions
- **Strongly Typed**: Each event carries specific domain information
- **Business Semantics**: Events represent meaningful business occurrences
- **Decoupled Communication**: Enables reactive programming patterns

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

The Domain layer defines repository contracts that are implemented in the Infrastructure layer.

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

#### ICountryRepository Interface
**File**: `Interfaces/Repositories/Settings/ICountryRepository.cs`

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

**Key Features**:
- **Async Operations**: All methods return Tasks for non-blocking execution
- **Cancellation Support**: CancellationToken support for operation cancellation
- **Domain-Specific Methods**: Business-specific query methods (e.g., GetByCodeAsync)
- **Pagination Support**: Built-in support for paged results
- ### Domain Services

Domain Services contain business logic that doesn't naturally fit within a single entity. They coordinate operations across multiple entities or handle complex business rules.

**Current Implementation**:

#### Stock Management Service
**File**: `Services/Inventory/IStockManagementService.cs`

```csharp
namespace GoldenFiberERP.Domain.Services.Inventory;

public interface IStockManagementService
{
    Task<bool> ReserveStockAsync(Product product, int quantity, string reason);
    Task ReleaseStockAsync(Product product, int quantity, string reason);
    Task AdjustStockAsync(Product product, int quantity, string reason);
    bool HasSufficientStock(Product product, int requiredQuantity);
    bool IsLowStock(Product product);
    Task TransferStockAsync(Product fromProduct, Product toProduct, int quantity, string reason);
}
```

**Purpose**:
- Coordinates complex stock operations across multiple products
- Implements business rules for stock reservations and transfers
- Handles stock validation and low-stock detection logic
- Ensures data consistency during stock movements

#### Pricing Service
**File**: `Services/Pricing/IPricingService.cs`

```csharp
namespace GoldenFiberERP.Domain.Services.Pricing;

public interface IPricingService
{
    Money CalculatePrice(Money basePrice, int customerId, int quantity, string currency = "USD");
    decimal CalculateDiscountPercentage(int customerId, int quantity);
    Money ApplyDiscounts(Money originalPrice, List<DiscountRule> discounts);
    bool ValidatePricingRules(Money price, Product product);
}
```

**Purpose**:
- Implements complex pricing calculations with customer-specific rules
- Handles quantity-based discounts and promotional pricing
- Validates pricing rules and business constraints
- Coordinates between ValueObjects (Money) and business logic

**Key Characteristics of Domain Services**:
- **Stateless**: No internal state, pure business logic
- **Domain-Focused**: Express business concepts and rules
- **Coordinating**: Orchestrate operations across multiple entities
- **Interface-Based**: Defined as interfaces in Domain, implemented in Infrastructure

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
