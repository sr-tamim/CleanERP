# System Health Module - Default Launch Configuration

## Overview

The GoldenFiberERP API is now configured to launch directly to the **System Health module** in Swagger UI, providing immediate access to health monitoring and diagnostic endpoints.

## Configuration Details

### 1. Default Module Selection
The System Health module (`⚙️ System & Health`) is now the **first endpoint** in the Swagger configuration, making it the default selection when Swagger UI loads.

**Location**: `Extensions/SwaggerExtensions.cs`
```csharp
// System & Health module first for default selection
options.SwaggerEndpoint("/swagger/system/swagger.json", "⚙️ System & Health");
options.SwaggerEndpoint("/swagger/v1/swagger.json", "🏢 API Overview");
// ... other modules follow
```

### 2. Launch Settings
The application is configured to automatically open the browser to the Swagger UI on launch.

**Location**: `Properties/launchSettings.json`
```json
"launchUrl": "swagger"
```

### 3. Enhanced Description
The System Health module has an enhanced description indicating it's the default launch module.

**Location**: `wwwroot/swagger-custom.js`
```javascript
'system': '🏥 Health checks, system monitoring, and administrative functions - Default launch module for system status'
```

## Available Health Endpoints

When the System Health module loads by default, users will see:

### Health Controller Endpoints
- `GET /api/health` - Basic API health check
- `GET /api/health/database` - Database connectivity health
- `GET /api/health/database/connection` - Detailed database connection test
- `GET /api/health/database/info` - Database information (masked credentials)
- `GET /api/health/database/schema` - Schema validation for manual management
- `GET /api/health/detailed` - Comprehensive system health check

## Benefits of System Health Default Launch

### 1. **Immediate System Status**
- Developers and operators get instant access to system health
- Quick verification that all services are running correctly
- Immediate visibility into any system issues

### 2. **DevOps Friendly**
- Perfect for development environment health checks
- Useful for deployment verification
- Supports monitoring and alerting workflows

### 3. **Clean Architecture Compliance**
- Health checks follow proper Clean Architecture patterns
- Application layer queries with infrastructure implementations
- No business logic in presentation layer

### 4. **Production Ready**
- Comprehensive health monitoring endpoints
- Proper error handling and status codes
- Detailed diagnostics without exposing sensitive data

## Usage

### Starting the Application
```bash
# Any of these commands will launch with System Health module selected
dotnet run --project src/Presentation/GoldenFiberERP.API/GoldenFiberERP.API.csproj

# Or using launch profiles
dotnet run --launch-profile https
dotnet run --launch-profile http
```

### Expected Behavior
1. **Browser Auto-Opens**: Navigates to `http://localhost:5000/swagger`
2. **System Health Selected**: Module dropdown shows "⚙️ System & Health" as selected
3. **Health Endpoints Visible**: Only health monitoring endpoints are displayed
4. **Module Switching**: Users can still switch to other modules as needed

### Keyboard Shortcuts (Still Available)
- `Alt + M`: Focus module selector
- `Alt + O`: Switch to API Overview
- `Alt + I`: Switch to Inventory module
- `Alt + A`: Switch to Authentication module
- `Alt + S`: Switch to Sales module (can be added)

## Module Organization

The complete module order is now:
1. **⚙️ System & Health** *(Default - Health monitoring)*
2. **🏢 API Overview** *(All endpoints)*
3. **📦 Inventory Management** *(Products, Stock)*
4. **🔐 Authentication & Authorization** *(Users, Security)*
5. **💰 Sales Management** *(Orders, Customers)*
6. **🏭 Manufacturing Operations** *(Production)*
7. **💳 Financial Management** *(Accounting, Payments)*

## Testing the Configuration

To verify the System Health default launch works correctly:

1. **Start Application**: `dotnet run`
2. **Verify Auto-Launch**: Browser opens to Swagger UI
3. **Check Default Module**: "⚙️ System & Health" is pre-selected
4. **Test Endpoints**: Health endpoints are visible and functional
5. **Test Module Switching**: Can navigate to other modules

## Maintenance

### Adding New Health Endpoints
New health endpoints added to `HealthController` will automatically appear in the System Health module due to the controller naming convention in `SwaggerExtensions.cs`.

### Changing Default Module
To change the default module, simply reorder the `SwaggerEndpoint` calls in `SwaggerExtensions.cs` - the first endpoint becomes the default.

### Customizing Launch Behavior
The launch behavior can be customized in `launchSettings.json` by modifying the `launchUrl` property for different scenarios:
- `"launchUrl": "swagger"` - Default overview (current)
- `"launchUrl": "swagger/?urls.primaryName=⚙️ System & Health"` - Direct to specific module
- `"launchUrl": "api/health"` - Direct to health endpoint (no UI)

This configuration provides an optimal developer experience with immediate access to system health monitoring while maintaining full flexibility to access other API modules as needed.
