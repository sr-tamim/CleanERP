# GoldenFiberERP - Architecture Documentation

This folder contains comprehensive architecture documentation for the GoldenFiberERP system.

## Documentation Overview

- **[System Overview](./01-system-overview.md)** - High-level system description and objectives
- **[Clean Architecture](./02-clean-architecture.md)** - Detailed explanation of the layered architecture
- **[Project Structure](./03-project-structure.md)** - Complete project structure and organization
- **[Domain Layer](./04-domain-layer.md)** - Domain entities, interfaces, and business rules
- **[Application Layer](./05-application-layer.md)** - Use cases, application services, and DTOs
- **[Infrastructure Layer](./06-infrastructure-layer.md)** - Data access, external services, and implementations
- **[Presentation Layer](./07-presentation-layer.md)** - API controllers, web interfaces, and presentation logic
- **[Design Patterns](./08-design-patterns.md)** - Implemented design patterns and architectural decisions
- **[Technology Stack](./09-technology-stack.md)** - Technologies, frameworks, and tools used
- **[Deployment Architecture](./10-deployment-architecture.md)** - Docker containerization and deployment strategy

## Development Guidelines

- **[Feature Implementation Guide](../development/feature-implementation-guide.md)** - Step-by-step guide for implementing new features/modules following Clean Architecture principles

## Architecture Principles

The GoldenFiberERP system follows Clean Architecture principles with clear separation of concerns:

1. **Dependency Inversion** - Dependencies point inward toward the domain
2. **Single Responsibility** - Each layer has a specific responsibility
3. **Interface Segregation** - Small, focused interfaces
4. **Domain-Centric Design** - Business logic is isolated and protected
5. **Testability** - Architecture supports comprehensive testing strategies

## Getting Started

Start with the [System Overview](./01-system-overview.md) to understand the high-level architecture, then proceed through the documentation in order for a complete understanding of the system design.
