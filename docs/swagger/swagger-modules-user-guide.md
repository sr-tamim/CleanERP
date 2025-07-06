# Module-wise Swagger Documentation

This implementation provides separate Swagger documentation pages for each business module in GoldenFiberERP, allowing for better organization and navigation of API endpoints.

## Features

### 🔧 Modular Organization
Each business domain has its own dedicated Swagger documentation:
- **API Overview** - Complete API documentation
- **Inventory Management** - Product, stock, and inventory APIs
- **Authentication & Authorization** - Security and user management APIs
- **Sales Management** - Order processing and customer management APIs
- **Manufacturing Operations** - Production and work order APIs
- **Financial Management** - Accounting and payment APIs
- **System & Health** - Monitoring and diagnostic APIs

### 🎨 Enhanced UI
- Custom CSS styling for better visual experience
- Prominent module selector dropdown in the top bar
- Color-coded modules for easy identification
- Improved navigation and readability

### 🔒 Security Integration
- JWT Bearer token authentication support
- Security scheme definitions for all modules
- Consistent authorization across all endpoints

## Usage

### 1. Access Swagger UI
Navigate to: `https://localhost:7266/swagger` (or `http://localhost:5000/swagger`)

### 2. Select Module
Use the dropdown in the top bar to switch between different modules:
- Select "API Overview" to see all endpoints
- Choose specific modules to focus on particular business domains

### 3. Test APIs
- Use the built-in testing interface
- Add Bearer token for authenticated endpoints
- View detailed request/response schemas

## Implementation Details

### Module Classification
Controllers are automatically assigned to modules based on their names:

```csharp
// Inventory Module
ProductsController, InventoryController, StockController, etc.

// Auth Module  
AuthController, UsersController, RolesController, etc.

// Sales Module
SalesController, OrdersController, CustomersController, etc.

// And so on...
```

### Adding New Controllers
When creating new controllers, they will automatically appear in the appropriate module based on their name. To assign a controller to a specific module:

1. **Name Convention**: Use descriptive controller names (e.g., `ManufacturingController`)
2. **Manual Assignment**: Modify the helper methods in `SwaggerExtensions.cs` if needed
3. **Tags**: Add `[Tags("Module Name")]` attribute for grouping within modules

### Configuration Files
- **SwaggerExtensions.cs** - Main configuration and module definitions
- **swagger-custom.css** - Custom styling for enhanced UI
- **Program.cs** - Integration with the application pipeline

## Benefits

### 🏗️ Clean Architecture Compliance
- All Swagger configuration is in the Presentation layer
- No business logic mixed with documentation concerns
- Extensible and maintainable design

### 👥 Developer Experience
- Easy navigation between different API domains
- Focused documentation for specific workflows
- Reduced cognitive load when working with large APIs

### 📱 Scalability
- Easy to add new modules as the system grows
- Automatic controller assignment based on naming conventions
- Consistent documentation patterns across all modules

## Maintenance

### Adding New Modules
1. Add new OpenAPI document in `AddModularSwagger()` method
2. Create corresponding helper method (e.g., `IsNewModuleController()`)
3. Add module endpoint in `UseModularSwaggerUI()` method
4. Update controller classification logic

### Customizing Appearance
- Modify `swagger-custom.css` for visual changes
- Update OpenAPI info objects for module descriptions
- Add custom headers or footers as needed

## Production Considerations

### Security
- Remove sensitive information from documentation in production
- Configure proper CORS policies
- Implement rate limiting for documentation endpoints

### Performance
- Consider caching Swagger JSON generation
- Minify CSS files for production
- Use CDN for static assets if needed

### Monitoring
- Track usage of different modules
- Monitor API documentation access patterns
- Log errors in Swagger generation

---

## Example Module Structure

### Inventory Module
Includes all product and inventory-related endpoints:
- `/api/Products/*` - Product management
- `/api/Inventory/*` - Stock management  
- `/api/Warehouse/*` - Warehouse operations
- `/api/Suppliers/*` - Supplier management

### Sales Module
Includes all sales and customer-related endpoints:
- `/api/Sales/*` - Sales operations
- `/api/Orders/*` - Order management
- `/api/Customers/*` - Customer management
- `/api/Invoices/*` - Billing operations

This modular approach ensures that teams can focus on their specific domain APIs while maintaining a comprehensive overview when needed.
