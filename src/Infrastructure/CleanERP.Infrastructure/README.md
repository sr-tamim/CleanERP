# CleanERP.Infrastructure Layer

This is the Infrastructure layer of the CleanERP system, implementing external concerns and services required by the Application layer. Database-specific repositories live in the Persistence project.

## Structure

### Services
Implementation of common infrastructure services that don't require database access:

- **DateTimeService**: Implementation of `IDateTime` for system time operations
- **CurrentUserService**: Implementation of `ICurrentUserService` for user context
- **EmailService**: Email sending functionality (placeholder implementation)
- **FileService**: File storage and retrieval operations
- **CacheService**: In-memory caching implementation

## Key Features

### Service Implementations
- Clean implementations of Application layer interfaces
- Separation of concerns for different infrastructure needs
- Configurable and testable services

### File Management
- Secure file upload and storage
- Unique file naming to prevent conflicts
- Directory management and validation

### Caching
- In-memory cache with expiration support
- Thread-safe operations
- JSON serialization for complex objects

### Email Services
- Abstracted email sending interface
- Support for single and bulk recipients
- HTML and plain text support

## Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.Http.Abstractions` - HTTP context access
- `Microsoft.Extensions.Configuration.Abstractions` - Configuration binding
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI container
- `Microsoft.Extensions.Logging.Abstractions` - Logging support

## Usage

### Registration in Startup/Program.cs
```csharp
services.AddInfrastructure(configuration);
```

### Service Injection
```csharp
public class SomeService
{
    private readonly IDateTime _dateTime;
    private readonly IEmailService _emailService;

    public SomeService(IDateTime dateTime, IEmailService emailService)
    {
        _dateTime = dateTime;
        _emailService = emailService;
    }
}
```

## Notes

- This layer contains general infrastructure services
- Database-specific implementations are in the Persistence layer
- All services are designed to be easily mockable for testing
- Email service contains placeholder implementation for demonstration
- File service uses local file system by default but can be extended for cloud storage
