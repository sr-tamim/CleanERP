# Project Structure

## Solution Overview

The GoldenFiberERP solution follows a clean, organized structure that supports the Clean Architecture principles and promotes maintainability and scalability.

## Root Directory Structure

```
GoldenFiberERP.sln              # Visual Studio solution file
├── docs/                       # Documentation
│   └── architecture/           # Architecture documentation
├── src/                        # Source code
│   ├── Core/                   # Core layers (Domain + Application)
│   ├── Infrastructure/         # Infrastructure layer
│   └── Presentation/           # Presentation layer
├── tests/                      # Test projects (planned)
├── docker-compose.yml          # Docker composition
├── docker-compose.override.yml # Development overrides
├── docker-compose.dcproj       # Docker compose project
├── .gitignore                  # Git ignore rules
├── .dockerignore              # Docker ignore rules
└── launchSettings.json        # Launch configuration
```

## Source Code Organization

### Core Layer (`src/Core/`)

#### Domain Project (`GoldenFiberERP.Domain`)
Contains the business entities, interfaces, and domain logic:

```
GoldenFiberERP.Domain/
├── GoldenFiberERP.Domain.csproj
├── Entities/
│   ├── Common/                 # Base entities and interfaces
│   │   ├── BaseEntity.cs       # Base class for all entities
│   │   ├── AuditableEntity.cs  # Auditable entity with tracking
│   │   └── ISoftDeleteEntity.cs # Soft delete interface
│   └── Inventory/              # Inventory domain entities
│       └── Product.cs          # Product entity
├── Enums/
│   └── UserRole.cs            # User role enumeration
├── Exceptions/
│   └── InsufficientStockException.cs # Domain exceptions
├── Interfaces/
│   └── Repositories/           # Repository contracts
│       ├── Common/
│       │   └── IBaseRepository.cs     # Base repository interface
│       └── Inventory/
│           └── IProductRepository.cs  # Product-specific repository
└── ValueObjects/              # Value objects (planned)
```

#### Application Project (`GoldenFiberERP.Application`)
Contains use cases, application services, and DTOs:

```
GoldenFiberERP.Application/
├── GoldenFiberERP.Application.csproj
├── Class1.cs                  # Placeholder (to be replaced)
├── Common/                    # Common application concerns (planned)
│   ├── Interfaces/            # Application interfaces
│   ├── Models/                # Common DTOs and models
│   ├── Exceptions/            # Application exceptions
│   └── Behaviors/             # Cross-cutting behaviors
├── Features/                  # Feature-based organization (planned)
│   ├── Inventory/
│   │   ├── Commands/          # Command handlers
│   │   ├── Queries/           # Query handlers
│   │   └── DTOs/              # Data transfer objects
│   ├── Manufacturing/         # Manufacturing features
│   ├── Sales/                 # Sales features
│   └── Financial/             # Financial features
└── Services/                  # Application services (planned)
    ├── IEmailService.cs
    ├── IFileService.cs
    └── INotificationService.cs
```

### Infrastructure Layer (`src/Infrastructure/`)

**Status**: Planned for future implementation

```
GoldenFiberERP.Infrastructure/ (planned)
├── GoldenFiberERP.Infrastructure.csproj
├── Data/                      # Data access implementations
│   ├── Contexts/
│   │   └── ApplicationDbContext.cs
│   ├── Configurations/        # Entity configurations
│   │   ├── ProductConfiguration.cs
│   │   └── BaseEntityConfiguration.cs
│   ├── Repositories/          # Repository implementations
│   │   ├── BaseRepository.cs
│   │   └── ProductRepository.cs
│   └── Migrations/            # EF Core migrations
├── Services/                  # External service implementations
│   ├── EmailService.cs
│   ├── FileService.cs
│   └── NotificationService.cs
├── Identity/                  # Identity and authentication
│   ├── Services/
│   │   ├── AuthenticationService.cs
│   │   └── AuthorizationService.cs
│   └── Models/
│       ├── ApplicationUser.cs
│       └── ApplicationRole.cs
└── External/                  # External API integrations
    ├── PaymentGateway/
    ├── ShippingProviders/
    └── TaxServices/
```

### Presentation Layer (`src/Presentation/`)

#### API Project (`GoldenFiberERP.API`)
Web API for external communication:

```
GoldenFiberERP.API/
├── GoldenFiberERP.API.csproj
├── Program.cs                 # Application entry point
├── appsettings.json          # Configuration (ignored by git)
├── appsettings.Development.json # Development configuration
├── Dockerfile                # Container definition
├── .dockerignore             # Docker ignore rules
├── Controllers/              # API controllers
│   └── WeatherForecastController.cs # Sample controller
├── Models/                   # API models (planned)
│   ├── Requests/             # Request DTOs
│   └── Responses/            # Response DTOs
├── Middleware/               # Custom middleware (planned)
│   ├── ErrorHandlingMiddleware.cs
│   ├── AuthenticationMiddleware.cs
│   └── LoggingMiddleware.cs
├── Filters/                  # Action filters (planned)
│   ├── ValidationFilter.cs
│   └── AuthorizationFilter.cs
├── Configuration/            # Startup configuration (planned)
│   ├── ServicesConfiguration.cs
│   └── MiddlewareConfiguration.cs
├── Properties/
│   └── launchSettings.json   # Launch profiles
└── WeatherForecast.cs        # Sample model (to be removed)
```

## Project Dependencies

### Dependency Hierarchy
```
GoldenFiberERP.API
    ↓ (references)
GoldenFiberERP.Application
    ↓ (references)
GoldenFiberERP.Domain

GoldenFiberERP.Infrastructure
    ↓ (references)
GoldenFiberERP.Application
    ↓ (references)
GoldenFiberERP.Domain
```

### Current Project References
Based on the solution file analysis:

- **GoldenFiberERP.API**: Currently has no project references (needs to reference Application layer)
- **GoldenFiberERP.Domain**: Standalone project (correct)
- **GoldenFiberERP.Application**: Currently has no references (should reference Domain)

## Configuration Files

### Solution Level
- **GoldenFiberERP.sln**: Visual Studio solution configuration
- **docker-compose.yml**: Main Docker composition
- **docker-compose.override.yml**: Development-specific overrides
- **docker-compose.dcproj**: Docker Compose project file

### Project Level
- **{Project}.csproj**: MSBuild project files
- **appsettings.json**: Application configuration
- **launchSettings.json**: Development launch profiles

### Development Tools
- **.gitignore**: Git version control exclusions
- **.dockerignore**: Docker build context exclusions

## Planned Directory Structure

### Tests (`tests/`)
```
tests/
├── UnitTests/
│   ├── GoldenFiberERP.Domain.UnitTests/
│   ├── GoldenFiberERP.Application.UnitTests/
│   └── GoldenFiberERP.Infrastructure.UnitTests/
├── IntegrationTests/
│   ├── GoldenFiberERP.API.IntegrationTests/
│   └── GoldenFiberERP.Infrastructure.IntegrationTests/
└── ArchitectureTests/
    └── GoldenFiberERP.ArchitectureTests/
```

### Scripts (`scripts/`)
```
scripts/
├── build/                     # Build scripts
├── deployment/                # Deployment scripts
├── database/                  # Database scripts
└── development/               # Development utilities
```

### Documentation (`docs/`)
```
docs/
├── architecture/              # Architecture documentation
├── api/                       # API documentation
├── deployment/                # Deployment guides
├── development/               # Development guides
└── user/                      # User documentation
```

## Naming Conventions

### Projects
- **Format**: `{Company}.{Product}.{Layer}`
- **Examples**: 
  - `GoldenFiberERP.Domain`
  - `GoldenFiberERP.Application`
  - `GoldenFiberERP.Infrastructure`
  - `GoldenFiberERP.API`

### Folders
- **PascalCase** for all folder names
- **Descriptive names** that clearly indicate purpose
- **Plural forms** for collections (e.g., `Entities`, `Controllers`)

### Files
- **PascalCase** for all file names
- **Meaningful names** that describe the content
- **Appropriate suffixes** (e.g., `Controller`, `Service`, `Repository`)

## Build and Output Structure

### Build Artifacts
```
{Project}/
├── bin/                       # Compiled assemblies
│   ├── Debug/
│   └── Release/
└── obj/                       # Build intermediate files
    ├── Debug/
    └── Release/
```

### Docker Context
- **Build Context**: Each API project has its own Dockerfile
- **Multi-stage Build**: Optimized for production deployment
- **Development Override**: Local development configuration

## Development Workflow

### Local Development
1. Clone the repository
2. Open `GoldenFiberERP.sln` in Visual Studio
3. Build the solution
4. Run using Docker Compose or IIS Express

### Adding New Features
1. Create entities in Domain layer
2. Add interfaces for repositories
3. Implement application services
4. Add infrastructure implementations
5. Create API controllers
6. Write tests

### Project Organization Benefits

1. **Clear Separation**: Each layer has its own project
2. **Dependency Control**: Project references enforce architecture
3. **Independent Deployment**: Layers can be deployed separately
4. **Testing Isolation**: Each layer can be tested independently
5. **Team Collaboration**: Different teams can work on different layers
