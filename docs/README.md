# GoldenFiberERP Documentation

This folder contains all project documentation organized by topic and purpose.

## 📁 Folder Structure

### [`architecture/`](architecture/)
Complete system architecture documentation following Clean Architecture principles:
- System overview and design decisions
- Layer-by-layer implementation details
- Technology stack and design patterns
- Deployment architecture

### [`swagger/`](swagger/)
Swagger/OpenAPI documentation and module-wise implementation:
- Module-wise Swagger implementation guide
- User guide for navigating Swagger UI
- Testing documentation
- System health default launch configuration

### [`health-checks/`](health-checks/)
Health monitoring and diagnostic system documentation:
- Clean Architecture health check implementation
- Database health endpoints
- Testing health check functionality

## 🚀 Quick Start

### For Developers
1. **Architecture Overview**: Start with [`architecture/02-clean-architecture.md`](architecture/02-clean-architecture.md)
2. **Project Structure**: Review [`architecture/03-project-structure.md`](architecture/03-project-structure.md)
3. **Swagger Usage**: See [`swagger/swagger-modules-user-guide.md`](swagger/swagger-modules-user-guide.md)

### For System Administrators
1. **Health Monitoring**: Review [`health-checks/`](health-checks/) folder
2. **Deployment**: See [`architecture/10-deployment-architecture.md`](architecture/10-deployment-architecture.md)

### For API Users
1. **Swagger Documentation**: See [`swagger/swagger-modules-user-guide.md`](swagger/swagger-modules-user-guide.md)
2. **API Testing**: Review [`swagger/swagger-modules-testing.md`](swagger/swagger-modules-testing.md)

## 📋 Documentation Index

### Architecture Documentation
| Document | Description |
|----------|-------------|
| [System Overview](architecture/01-system-overview.md) | High-level system design and business requirements |
| [Clean Architecture](architecture/02-clean-architecture.md) | Architecture principles and layer organization |
| [Project Structure](architecture/03-project-structure.md) | Folder structure and project organization |
| [Domain Layer](architecture/04-domain-layer.md) | Business entities and domain logic |
| [Application Layer](architecture/05-application-layer.md) | Use cases and application services |
| [Infrastructure Layer](architecture/06-infrastructure-layer.md) | Data access and external services |
| [Presentation Layer](architecture/07-presentation-layer.md) | API controllers and web interface |
| [Design Patterns](architecture/08-design-patterns.md) | Patterns used throughout the system |
| [Technology Stack](architecture/09-technology-stack.md) | Technologies, frameworks, and tools |
| [Deployment Architecture](architecture/10-deployment-architecture.md) | Deployment and infrastructure setup |

### Swagger Documentation
| Document | Description |
|----------|-------------|
| [User Guide](swagger/swagger-modules-user-guide.md) | How to use the module-wise Swagger UI |
| [Implementation](swagger/swagger-modules-implementation.md) | Technical implementation details |
| [Testing Guide](swagger/swagger-modules-testing.md) | Testing Swagger functionality |
| [System Health Launch](swagger/system-health-default-launch.md) | Default health module configuration |

### Health Checks Documentation
| Document | Description |
|----------|-------------|
| [Clean Architecture Implementation](health-checks/clean-architecture-health-checks.md) | Health checks following Clean Architecture |
| [Database Health Endpoints](health-checks/database-health-endpoints.md) | Database monitoring endpoints |
| [Testing Health Checks](health-checks/health-check-testing.md) | How to test health monitoring |

## 🏗️ Contributing to Documentation

### Adding New Documentation
1. **Choose appropriate folder** based on content type
2. **Follow naming convention**: `kebab-case.md`
3. **Update this README** to include new documents
4. **Cross-reference** related documents

### Documentation Standards
- Use clear, descriptive headings
- Include code examples where appropriate
- Add diagrams for complex concepts
- Keep documents focused on single topics
- Reference related documents

### Folder Guidelines

#### `architecture/`
- System design and architectural decisions
- Layer-specific implementation details
- Technology choices and patterns

#### `swagger/`
- OpenAPI/Swagger related documentation
- API documentation and usage guides
- Swagger UI customizations

#### `health-checks/`
- System monitoring and diagnostics
- Health check implementations
- Testing health endpoints

This organization ensures documentation is easy to find, navigate, and maintain as the project grows.
