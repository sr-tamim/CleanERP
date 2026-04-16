# CleanERP.Shared

This is the Shared library containing common utilities, constants, extensions, and models used across all layers of the CleanERP system.

## Structure

### Constants
- **ApplicationConstants**: System-wide constants including roles, permissions, validation rules, and default values
- Centralized configuration for business rules and system behavior

### Enums
- **CommonEnums**: Business domain enums (ProductStatus, OrderStatus, PaymentStatus, etc.)
- Standardized status and type definitions across the application

### Extensions
- **CommonExtensions**: Extension methods for common types
  - String extensions (ToTitleCase, Truncate, ToSafeFileName)
  - DateTime extensions (ToFriendlyString, IsToday, IsThisWeek)
  - Decimal extensions (ToCurrency, ToPercentage)
  - Enum extensions (GetDescription, ToEnum)

### Utilities
- **CommonUtilities**: Helper classes for common operations
  - **PasswordHelper**: Password hashing and generation
  - **CodeGenerator**: Generate product codes, order numbers, barcodes
  - **ValidationHelper**: Email, phone, SKU validation
  - **DateTimeHelper**: Date range calculations, business days

### Models
- **CommonModels**: Shared data models
  - **PaginationRequest/Response**: Standardized pagination
  - **SortRequest**: Sorting parameters
  - **FilterRequest**: Filtering parameters
  - **AuditInfo**: Audit trail information
  - **NotificationModel**: System notifications
  - **FileUploadModel**: File handling

### Exceptions
- **SharedExceptions**: Custom exception types
  - **BusinessException**: Business rule violations
  - **ConfigurationException**: Configuration errors
  - **ExternalServiceException**: Third-party service failures
  - **ConcurrencyException**: Concurrent access conflicts
  - **SecurityException**: Security violations

## Key Features

### Cross-Layer Utilities
- Reusable across Domain, Application, Infrastructure, and Presentation layers
- No dependencies on other project layers
- Pure utility functions and constants

### Business Domain Support
- ERP-specific enums and constants
- Textile/fiber industry relevant utilities
- Standardized business logic helpers

### Development Productivity
- Common extension methods for cleaner code
- Standardized models for consistent APIs
- Utility functions for frequent operations

## Usage Examples

### Using Constants
```csharp
// Role-based authorization
if (user.Role == ApplicationConstants.Roles.Administrator)
{
    // Admin operations
}

// Validation
if (product.Name.Length > ApplicationConstants.Validation.MaxProductNameLength)
{
    // Handle validation error
}
```

### Using Extensions
```csharp
// String extensions
var fileName = userInput.ToSafeFileName();
var title = productName.ToTitleCase();

// DateTime extensions
var friendlyDate = order.CreatedAt.ToFriendlyString(); // "2 hours ago"
var isRecent = order.CreatedAt.IsToday();

// Decimal extensions
var price = product.Price.ToCurrency(); // "$25.50"
```

### Using Utilities
```csharp
// Generate codes
var productCode = CodeGenerator.GenerateProductCode("FABRIC");
var orderNumber = CodeGenerator.GenerateOrderNumber();

// Validation
var isValid = ValidationHelper.IsValidEmail(email);
var isValidSKU = ValidationHelper.IsValidSKU(sku);

// Date calculations
var businessDays = DateTimeHelper.GetBusinessDaysBetween(startDate, endDate);
var monthStart = DateTimeHelper.GetStartOfMonth(DateTime.Now);
```

### Using Models
```csharp
// Pagination
var request = new PaginationRequest { PageNumber = 1, PageSize = 20 };
var response = new PaginationResponse<Product>
{
    Data = products,
    TotalCount = totalCount,
    PageNumber = request.PageNumber,
    PageSize = request.PageSize
};

// Filtering
var filter = new FilterRequest
{
    SearchTerm = "cotton",
    FromDate = DateTime.Today.AddDays(-30)
};
```

## Dependencies

- **No external dependencies** - Pure .NET 8 library
- **No references to other project layers** - Maintains clean architecture

## Notes

- This library should remain dependency-free to be usable across all layers
- Add new utilities and constants as the system grows
- Keep business logic in appropriate layers (Domain/Application)
- Use this for pure utility functions and shared models only

## Contributing

When adding new shared components:
1. Ensure they are truly cross-cutting concerns
2. Keep them stateless and pure functions where possible
3. Add appropriate unit tests
4. Update documentation and examples
