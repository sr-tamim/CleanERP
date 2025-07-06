# GoldenFiberERP.API - Presentation Layer

This is the Presentation layer of the GoldenFiberERP system, built with ASP.NET Core Web API. It serves as the entry point for all client interactions and handles HTTP requests/responses.

## Structure

### Controllers
- **ProductsController**: RESTful API endpoints for product management
  - GET /api/products - Get all products
  - GET /api/products/{id} - Get product by ID
  - POST /api/products - Create new product
  - PUT /api/products/{id} - Update product
  - DELETE /api/products/{id} - Delete product

### Middleware
- **GlobalExceptionHandler**: Centralized exception handling
  - ValidationException -> 400 Bad Request
  - NotFoundException -> 404 Not Found
  - Generic exceptions -> 500 Internal Server Error

### Models
- **ApiResponse<T>**: Standardized API response format
- **DTOs**: Data Transfer Objects from Application layer

## Key Features

### Clean Architecture Integration
- Proper dependency injection setup
- Layer separation and dependency inversion
- MediatR integration for CQRS pattern

### API Documentation
- Swagger/OpenAPI integration
- Automatic API documentation generation
- Development-friendly UI at `/swagger`

### Error Handling
- Global exception handling middleware
- Consistent error response format
- Proper HTTP status codes

### Configuration
- Environment-specific settings
- Database connection strings
- CORS configuration for development

## Dependencies

### NuGet Packages
- `Microsoft.EntityFrameworkCore.Design` - EF Core design-time tools
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication (for future use)
- `FluentValidation.AspNetCore` - Model validation

### Project References
- `GoldenFiberERP.Application` - Business logic and use cases
- `GoldenFiberERP.Infrastructure` - Infrastructure services
- `GoldenFiberERP.Persistence` - Database access

## Configuration

### Database Connection
Configure in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GoldenFiberERP;Username=postgres;Password=postgres"
  }
}
```

### Running the API
```bash
dotnet run --project src/Presentation/GoldenFiberERP.API
```

### API Endpoints

#### Products API
- **GET** `/api/products` - Retrieve all products
- **GET** `/api/products/{id}` - Retrieve specific product
- **POST** `/api/products` - Create new product
- **PUT** `/api/products/{id}` - Update existing product  
- **DELETE** `/api/products/{id}` - Delete product

#### Sample Request Bodies

**Create Product:**
```json
{
  "name": "Cotton Fabric",
  "description": "High-quality cotton fabric",
  "price": 25.50,
  "stockQuantity": 100,
  "sku": "CTN-001"
}
```

**Update Product:**
```json
{
  "name": "Premium Cotton Fabric",
  "description": "Premium grade cotton fabric",
  "price": 30.00,
  "stockQuantity": 150,
  "sku": "CTN-001"
}
```

## Development

### Swagger UI
Access API documentation at: `https://localhost:5001/swagger`

### CORS
Configured to allow all origins in development. Update for production use.

### Logging
- Detailed logging in development
- EF Core command logging enabled
- Structured logging with Serilog (can be added)

## Production Considerations

- Update CORS policy for specific domains
- Add authentication and authorization
- Implement rate limiting
- Add API versioning
- Configure proper logging providers
- Set up health checks
- Configure HTTPS certificates

## Testing

The API endpoints can be tested using:
- Swagger UI (built-in)
- Postman
- curl commands
- Automated integration tests

## Notes

This is a scaffold implementation providing:
- ✅ Clean Architecture setup
- ✅ CRUD operations for Products
- ✅ Global exception handling
- ✅ API documentation
- ✅ Environment configuration
- ✅ Consistent response format

Ready for extension with additional controllers, authentication, and business features.
