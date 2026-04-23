# CleanERP.CompositionRoot

## Overview

This project implements the **Composition Root** pattern for the CleanERP application. It serves as the single place where all dependencies across all layers are wired together, maintaining proper separation of concerns while enabling enterprise-scale dependency injection.

## Purpose

The composition root pattern provides several key benefits:

1. **Separation of Concerns**: Keeps dependency configuration separate from business logic and presentation layers
2. **Single Responsibility**: Has one job - wire dependencies together
3. **Clean Architecture**: Presentation layer doesn't directly reference infrastructure layers
4. **Enterprise Scale**: Supports modular registration for large ERP systems
5. **Testability**: Makes it easy to create different compositions for testing

## Architecture

```
┌─────────────────────────────────────────┐
│           Presentation Layer            │
│         (CleanERP.API)           │
│                    │                    │
│         References │ Only                │
│                    ▼                    │
│      ┌─────────────────────────────┐    │
│      │      Composition Root       │    │
│      │  (CleanERP.           │    │
│      │   CompositionRoot)          │    │
│      │                             │    │
│      │  ┌─────────────────────┐    │    │
│      │  │   AddApplication()  │    │    │
│      │  └─────────────────────┘    │    │
│      │  ┌─────────────────────┐    │    │
│      │  │ AddInfrastructure() │    │    │
│      │  └─────────────────────┘    │    │
│      │  ┌─────────────────────┐    │    │
│      │  │  AddPersistence()   │    │    │
│      │  └─────────────────────┘    │    │
│      └─────────────────────────────┘    │
└─────────────────────────────────────────┘
```

## Project Structure

```
CleanERP.CompositionRoot/
├── CleanERP.CompositionRoot.csproj  # Project configuration
├── DependencyInjection.cs                 # Main composition root
├── Modules/                               # Module-specific registrations (future)
│   ├── InventoryModule.cs                 # Inventory module registration
│   ├── SalesModule.cs                     # Sales module registration
│   └── ManufacturingModule.cs             # Manufacturing module registration
└── README.md                              # This file
```

## Key Features

### 1. Clean Layer Separation
- **Presentation Layer** only references **Composition Root**
- **Composition Root** references all layers
- **Infrastructure** layers don't leak into presentation

### 2. Modular Registration
- Core services registered first
- Business modules registered conditionally
- Cross-cutting concerns handled separately

### 3. Configuration-Driven
- Modules can be enabled/disabled via configuration
- Environment-specific compositions
- Feature flags support

## Usage

### Basic Registration
```csharp
// In Program.cs (Presentation Layer)
builder.Services.AddCleanERP(builder.Configuration);
```

### Module Configuration
```json
// In appsettings.json
{
  "Modules": {
    "Manufacturing": {
      "Enabled": true
    },
    "Financial": {
      "Enabled": false
    },
    "HumanResources": {
      "Enabled": true
    }
  }
}
```

## Benefits for ERP Systems

### 1. Enterprise Scale
- Supports 100+ services across multiple modules
- Modular registration prevents bloated startup
- Conditional module loading for different deployments

### 2. Team Collaboration
- Different teams can work on modules independently
- Clear boundaries between business domains
- Centralized dependency management

### 3. Deployment Flexibility
- Different environments can have different module configurations
- Microservice extraction becomes easier
- A/B testing of modules

### 4. Maintenance
- Single place to understand all dependencies
- Easy to add new modules
- Clear dependency hierarchy

## Future Enhancements

### 1. Module-Specific Registrations
```csharp
// Modules/InventoryModule.cs
public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Inventory-specific services
        return services;
    }
}
```

### 2. Configuration Validation
```csharp
// Validate module configurations at startup
services.Configure<ModuleOptions>(configuration.GetSection("Modules"));
services.AddOptions<ModuleOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 3. Health Check Integration
```csharp
// Module-specific health checks
services.AddHealthChecks()
    .AddCheck<InventoryHealthCheck>("inventory")
    .AddCheck<SalesHealthCheck>("sales");
```

## Dependencies

This project references:
- **Core Layers**: Application, Domain, Shared
- **Infrastructure Layers**: Infrastructure, Persistence
- **Microsoft Extensions**: DependencyInjection, Configuration, HealthChecks

## Notes

- This project should be the **ONLY** place where infrastructure layers are referenced
- Presentation layers should **NEVER** directly reference infrastructure
- All module registration should flow through this composition root
- Keep this project focused on dependency injection only - no business logic
