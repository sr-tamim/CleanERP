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

#### Shared Project (`GoldenFiberERP.Shared`)
Contains shared utilities and common functionality:

```
GoldenFiberERP.Shared/
├── GoldenFiberERP.Shared.csproj
├── Constants/                 # Application constants
├── Extensions/                # Extension methods
├── Helpers/                   # Utility helpers
└── Common/                    # Common shared functionality
```

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
│   ├── Inventory/              # Inventory domain entities
│   │   └── Product.cs          # Product entity
│   └── Settings/               # Settings domain entities
│       └── Country.cs          # Country entity
├── Enums/
│   └── UserRole.cs            # User role enumeration
├── Events/                     # Domain events
│   └── Settings/               # Settings events
│       └── CountryEvents.cs    # Country domain events
├── Exceptions/
│   └── InsufficientStockException.cs # Domain exceptions
├── Interfaces/
│   └── Repositories/           # Repository contracts
│       ├── Common/
│       │   └── IBaseRepository.cs     # Base repository interface
│       ├── Inventory/
│       │   └── IProductRepository.cs  # Product-specific repository
│       └── Settings/
│           └── ICountryRepository.cs  # Country repository interface
├── Services/                   # Domain services
│   ├── Inventory/              # Inventory-related domain services
│   │   └── IStockManagementService.cs # Stock management business logic
│   └── Pricing/                # Pricing-related domain services
│       └── IPricingService.cs  # Pricing calculation business logic
├── Specifications/             # Domain specifications
└── ValueObjects/              # Value objects
```

#### Application Project (`GoldenFiberERP.Application`)
Contains use cases, application services, and DTOs:

```
GoldenFiberERP.Application/
├── GoldenFiberERP.Application.csproj
├── DependencyInjection.cs     # Dependency injection configuration
├── README.md                  # Application layer documentation
├── Common/                    # Common application concerns
│   ├── Interfaces/            # Application interfaces
│   │   └── IApplicationDbContext.cs # Database context interface
│   ├── Models/                # Common DTOs and models
│   ├── Exceptions/            # Application exceptions
│   └── Behaviors/             # Cross-cutting behaviors
├── Features/                  # Feature-based organization
│   ├── Health/                # Health check features
│   ├── Inventory/             # Inventory features
│   │   ├── Commands/          # Command handlers
│   │   ├── Queries/           # Query handlers
│   │   └── DTOs/              # Data transfer objects
│   └── Settings/              # Settings features
│       ├── Commands/          # Country commands
│       ├── Queries/           # Country queries
│       ├── DTOs/              # Country DTOs
│       ├── Validators/        # Input validators
│       └── Mappers/           # AutoMapper profiles
└── Services/                  # Application services
```

### Infrastructure Layer (`src/Infrastructure/`)

#### Infrastructure Project (`GoldenFiberERP.Infrastructure`)
Contains external service implementations and infrastructure concerns:

```
GoldenFiberERP.Infrastructure/
├── GoldenFiberERP.Infrastructure.csproj
├── DependencyInjection.cs     # Service registration
├── README.md                  # Infrastructure documentation
├── Extensions/                # Extension methods
├── Persistence/               # Data access implementations
│   └── Repositories/          # Repository implementations
│       ├── Common/            # Base repository implementations
│       ├── Inventory/         # Inventory repositories
│       └── Settings/          # Settings repositories
│           └── CountryRepository.cs # Country repository
└── Services/                  # External service implementations
    ├── EmailService.cs        # Email service implementation
    ├── FileService.cs         # File service implementation
    └── NotificationService.cs # Notification service implementation
```

#### Persistence Project (`GoldenFiberERP.Persistence`)
Contains database-specific implementations:

```
GoldenFiberERP.Persistence/
├── GoldenFiberERP.Persistence.csproj
├── DependencyInjection.cs     # Persistence service registration
├── README.md                  # Persistence documentation
├── Configurations/            # Entity configurations
│   └── Settings/              # Settings entity configurations
│       └── CountryConfiguration.cs # Country EF configuration
├── Contexts/                  # Database contexts
│   └── ApplicationDbContext.cs # Main database context
├── Repositories/              # Additional repository implementations
└── Seeders/                   # Database seeders
```
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
├── appsettings.json          # Production configuration
├── appsettings.Development.json # Development configuration
├── Dockerfile                # Container definition
├── GoldenFiberERP.API.http   # HTTP client test file
├── WeatherForecast.cs        # Sample model (to be removed)
├── Controllers/              # API controllers
│   ├── WeatherForecastController.cs # Sample controller
│   └── Settings/             # Settings controllers
│       └── CountriesController.cs # Country CRUD controller
├── Extensions/               # API extensions
│   ├── SwaggerExtensions.cs  # Swagger configuration
│   └── SwaggerOperationFilter.cs # Swagger operation filter
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
└── Properties/
    └── launchSettings.json   # Launch profiles
```

## Project Dependencies

### Dependency Hierarchy
```
GoldenFiberERP.API
    ↓ (references)
    ├── GoldenFiberERP.Application
    ├── GoldenFiberERP.Shared
    ├── GoldenFiberERP.Infrastructure
    └── GoldenFiberERP.Persistence

GoldenFiberERP.Application
    ↓ (references)
    └── GoldenFiberERP.Domain

GoldenFiberERP.Infrastructure
    ↓ (references)
    ├── GoldenFiberERP.Application
    └── GoldenFiberERP.Domain

GoldenFiberERP.Persistence
    ↓ (references)
    ├── GoldenFiberERP.Application
    └── GoldenFiberERP.Domain

GoldenFiberERP.Shared
    └── (standalone)

GoldenFiberERP.Domain
    └── (standalone)
```

### Current Project References
Based on the actual project file analysis:

- **GoldenFiberERP.API**: References Application, Shared, Infrastructure, and Persistence layers
- **GoldenFiberERP.Application**: References Domain layer
- **GoldenFiberERP.Infrastructure**: References Application and Domain layers
- **GoldenFiberERP.Persistence**: References Application and Domain layers
- **GoldenFiberERP.Shared**: Standalone project (no references)
- **GoldenFiberERP.Domain**: Standalone project (correct)

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
