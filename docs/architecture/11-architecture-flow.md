# Architecture and Request Flow Guide

This guide explains how GoldenFiberERP is structured, how requests flow through the system, and where to look for the key implementation pieces. It is written for junior developers who are new to Clean Architecture in this codebase.

## 1) Big Picture

GoldenFiberERP follows Clean Architecture with four layers:

- **Presentation**: HTTP endpoints, request/response formatting.
- **Composition Root**: The single place where all dependencies are wired together.
- **Application**: Use cases, CQRS handlers, validation, cross-cutting behaviors.
- **Domain**: Business entities and rules (no external dependencies).
- **Infrastructure/Persistence**: External services and database access (split by concern).

Dependency direction is always inward (outer layers can depend on inner layers, never the other way around).

## 2) Solution Structure (Actual Projects)

```
src/
  Presentation/GoldenFiberERP.API          # Web API
  CompositionRoot/GoldenFiberERP.CompositionRoot
  Core/GoldenFiberERP.Domain               # Entities + domain contracts
  Core/GoldenFiberERP.Application          # Use cases, handlers, DTOs
  Core/GoldenFiberERP.Shared               # Shared helpers/constants
  Infrastructure/GoldenFiberERP.Infrastructure
  Infrastructure/GoldenFiberERP.Persistence
```

## 3) Composition Root (Where Everything Is Wired)

- **Entry point**: `src/Presentation/GoldenFiberERP.API/Program.cs`
- **Composition root**: `src/CompositionRoot/GoldenFiberERP.CompositionRoot/DependencyInjection.cs`

`Program.cs` calls:

```
builder.Services.AddGoldenFiberERP(builder.Configuration);
```

That method registers:

1) **Application layer** (`AddApplication`)
2) **Infrastructure layer** (`AddInfrastructure`)
3) **Persistence layer** (`AddPersistence`)

This keeps dependency wiring in one place and prevents the API from directly referencing infrastructure types.

## 4) Request Flow (Typical API Endpoint)

Example: `GET /api/settings/countries` handled by:
- `src/Presentation/GoldenFiberERP.API/Controllers/Settings/CountriesController.cs`
- `src/Core/GoldenFiberERP.Application/Features/Settings/Queries/GetCountriesQuery.cs`

Flow:

1) **HTTP request** hits `CountriesController.GetCountries`.
2) Controller builds a `GetCountriesQuery` and sends it with MediatR:
   - `IMediator.Send(query)`
3) **Application handler** (`GetCountriesQueryHandler`) runs:
   - Uses `ICountryRepository` to query the database.
   - Applies filtering and pagination in the repository.
   - Maps entities to DTOs using AutoMapper.
   - Returns `Result<PagedResult<CountryDto>>`.
4) **Controller** converts `Result<T>` into a consistent API response using `BaseController.HandleResult`.

Key files:
- `src/Presentation/GoldenFiberERP.API/Controllers/Settings/CountriesController.cs`
- `src/Presentation/GoldenFiberERP.API/Controllers/Common/BaseController.cs`
- `src/Core/GoldenFiberERP.Application/Features/Settings/Queries/GetCountriesQuery.cs`
- `src/Core/GoldenFiberERP.Application/Common/Models/Result.cs`

## 5) Command Flow (Write Operations)

Example: `POST /api/settings/countries`:

1) Controller creates a `CreateCountryCommand`.
2) `CreateCountryCommandHandler` executes:
   - Validates business constraints (unique code checks).
   - Creates a domain entity via `Country.Create(...)`.
   - Saves using repository + unit of work.
3) Result is returned to the controller, which emits a 201.

Key files:
- `src/Core/GoldenFiberERP.Application/Features/Settings/Commands/CreateCountryCommand.cs`
- `src/Infrastructure/GoldenFiberERP.Persistence/Contexts/ApplicationDbContext.cs`
- `src/Infrastructure/GoldenFiberERP.Persistence/Repositories/Settings/CountryRepository.cs`

## 6) Cross-Cutting Behaviors (MediatR Pipeline)

The Application layer registers pipeline behaviors in:
- `src/Core/GoldenFiberERP.Application/DependencyInjection.cs`

Order matters; current order is:
1) `LoggingBehavior`
2) `PerformanceBehavior`
3) `ValidationBehavior`
4) `CachingBehavior`
5) `AuditingBehavior`
6) `NotificationBehavior`
7) `RetryBehavior`

Notes:
- Caching runs only for requests ending with `Query` and only if they implement `ICacheableRequest`.
- Validation uses FluentValidation validators registered in the Application assembly.

## 7) Persistence and Domain Event Flow

Database access is via EF Core `ApplicationDbContext` in Persistence:
- `src/Infrastructure/GoldenFiberERP.Persistence/Contexts/ApplicationDbContext.cs`

Key points:
- Implements `IUnitOfWork`.
- Auditing fields (`CreatedBy`, `UpdatedBy`, timestamps) are set in `SaveChangesAsync`.
- Domain events are dispatched via `IDomainEventService` when saving.
- Transactions are wrapped with `ExecuteInTransactionAsync(...)`.

Repositories are registered in:
- `src/Infrastructure/GoldenFiberERP.Persistence/DependencyInjection.cs`
- `src/Infrastructure/GoldenFiberERP.Infrastructure/DependencyInjection.cs`

## 8) Error Handling and API Responses

The API avoids per-controller try/catch by centralizing exception handling:
- `src/Presentation/GoldenFiberERP.API/Middleware/GlobalExceptionHandler.cs`

Controllers return `Result<T>` and `Result` from Application handlers, then wrap them as `ApiResponse` or `ApiResponse<T>`.

Key files:
- `src/Presentation/GoldenFiberERP.API/Models/ApiResponse.cs`
- `src/Presentation/GoldenFiberERP.API/Middleware/GlobalExceptionHandler.cs`

## 9) Health Check Flow (Clean Architecture)

Health endpoints are implemented as proper CQRS:

1) Controller calls MediatR query (`GetDatabaseHealthQuery`, etc.).
2) Application handler uses `IHealthCheckService`.
3) Persistence implements `HealthCheckService` with actual database logic.

Key files:
- `src/Presentation/GoldenFiberERP.API/Controllers/HealthController.cs`
- `src/Core/GoldenFiberERP.Application/Features/Health/Queries/*`
- `src/Infrastructure/GoldenFiberERP.Persistence/Services/HealthCheckService.cs`

Related docs:
- `docs/health-checks/clean-architecture-health-checks.md`
- `docs/health-checks/database-health-endpoints.md`

## 10) Swagger and Module Organization

Swagger is configured with module-wise docs (inventory, auth, sales, system, etc.):
- `src/Presentation/GoldenFiberERP.API/Extensions/SwaggerExtensions.cs`
- `docs/swagger/swagger-modules-implementation.md`

Controllers are categorized based on naming conventions via `SwaggerOperationFilter`.

## 11) Patterns Used

### Clean Architecture
- **Purpose**: Keep business rules independent of frameworks and IO.
- **How it’s used**: Domain/Application are core; Infrastructure/Persistence implement external concerns.
- **Why it helps**: Core logic is testable and stable even if tech changes.

### Composition Root
- **Purpose**: Centralize all dependency wiring.
- **How it’s used**: `AddGoldenFiberERP` registers Application, Infrastructure, Persistence.
- **Why it helps**: Prevents accidental cross‑layer references.

### CQRS
- **Purpose**: Separate read and write use cases.
- **How it’s used**: `Command` and `Query` records + handlers via MediatR.
- **Why it helps**: Clearer intent and easier scaling for reads vs writes.

### Repository Pattern
- **Purpose**: Hide data access details behind interfaces.
- **How it’s used**: Domain defines interfaces; Persistence implements with EF Core.
- **Why it helps**: Application doesn’t depend on EF or SQL.

### Unit of Work
- **Purpose**: Commit multiple changes as a single transaction.
- **How it’s used**: `IUnitOfWork.SaveChangesAsync()` in handlers.
- **Why it helps**: Keeps data consistent across multiple repository calls.

### Specification Pattern
- **Purpose**: Reuse query filters and ordering logic.
- **How it’s used**: Domain specs like `ActiveCountriesSpecification`.
- **Why it helps**: Avoids duplicating query logic in handlers.

### Pipeline Behaviors
- **Purpose**: Apply cross‑cutting logic around every request.
- **How it’s used**: MediatR behaviors for logging, validation, caching, retry.
- **Why it helps**: One place to change behavior for all handlers.

### Result Pattern
- **Purpose**: Standardize success/failure results.
- **How it’s used**: Handlers return `Result`/`Result<T>`.
- **Why it helps**: Consistent API responses and error handling.

### Domain Events
- **Purpose**: Signal that a business fact occurred.
- **How it’s used**: Entities raise events (e.g., `CountryCreatedEvent`).
- **Why it helps**: Decouples side effects (audit, notifications) from core logic.

### Value Objects
- **Purpose**: Model domain concepts as immutable types.
- **How it’s used**: Types like `Money`, `Email`, `ProductSku`.
- **Why it helps**: Centralizes validation and reduces primitive misuse.

## 12) AutoMapper and MediatR

### AutoMapper
- **Purpose**: Maps entities to DTOs and vice versa without repetitive code.
- **How it’s used**: Handlers inject `IMapper` and call `_mapper.Map<Dto>(entity)`.
- **Where configured**: `src/Core/GoldenFiberERP.Application/DependencyInjection.cs` registers all profiles.

### MediatR
- **Purpose**: Decouples controllers from business logic via CQRS handlers.
- **How it’s used**: Controllers call `_mediator.Send(commandOrQuery)`, handlers execute.
- **Pipeline**: Behaviors run before/after handlers (logging, validation, caching, etc.).

## 13) Configuration and Bootstrapping

Configuration is read from `appsettings.json` and environment overrides. The current code uses:
- `ConnectionStrings:DefaultConnection` for EF Core (`AddPersistence`).

`Program.cs` also:
- Initializes the database with `EnsureCreatedAsync`.
- Adds CORS policy `AllowAll`.
- Adds API filters, exception handler, and Swagger UI in development.

Key file:
- `src/Presentation/GoldenFiberERP.API/Program.cs`

## 14) Project Roles (Application, Infrastructure, Persistence)

### Application Project (`src/Core/GoldenFiberERP.Application`)
- **Purpose**: Holds use cases and business workflows without external dependencies.
- **What lives here**: CQRS commands/queries + handlers, DTOs, validators, pipeline behaviors, and application interfaces.
- **Why it matters**: Keeps business orchestration testable and independent from frameworks or storage.

### Infrastructure Project (`src/Infrastructure/GoldenFiberERP.Infrastructure`)
- **Purpose**: Implements external, non-DB services required by the Application layer.
- **What lives here**: Email/file/cache services, current user/date time services, domain event service.
- **Why it matters**: Keeps third-party or framework-specific code out of core logic.

### Persistence Project (`src/Infrastructure/GoldenFiberERP.Persistence`)
- **Purpose**: Owns database access and EF Core configuration.
- **What lives here**: `ApplicationDbContext`, EF configurations, repositories, seeders, and DB health checks.
- **Why it matters**: Isolates database details and makes it easy to switch or test storage.

## 15) How to Explain This to a Junior Dev

Quick script:

1) “Requests hit the API controller. Controllers do no business logic.”
2) “Controllers send commands/queries via MediatR.”
3) “Handlers in the Application layer orchestrate work.”
4) “Domain contains the business rules and entities.”
5) “Infrastructure and Persistence talk to external systems and the database.”
6) “The Composition Root wires all dependencies in one place.”

If you show them just three files first, use:
- `src/Presentation/GoldenFiberERP.API/Program.cs`
- `src/CompositionRoot/GoldenFiberERP.CompositionRoot/DependencyInjection.cs`
- `src/Core/GoldenFiberERP.Application/Features/Settings/Commands/CreateCountryCommand.cs`
