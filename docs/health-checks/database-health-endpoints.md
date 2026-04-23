# Health Check Endpoints

This document describes the comprehensive health check endpoints available in the CleanERP API, specifically designed for manual database schema management (no Entity Framework migrations). All health-related functionality is consolidated into a single `HealthController`.

## Overview

The system uses **PostgreSQL** as the database with **manual schema management**. All health check endpoints are designed to work without relying on Entity Framework migrations and are organized under the `/api/health` route.

## Endpoints

### 1. Basic API Health
**GET** `/api/health`

Returns basic API status without database connectivity.

```json
{
  "Status": "Healthy",
  "Timestamp": "2024-01-15T10:30:00Z",
  "Application": "CleanERP API",
  "Version": "1.0.0",
  "Environment": "Development"
}
```

### 2. Database Health Check
**GET** `/api/health/database`

Tests basic database connectivity and provides PostgreSQL-specific information.

```json
{
  "Status": "Healthy",
  "Timestamp": "2024-01-15T10:30:00Z",
  "Component": "Database",
  "DatabaseName": "cleanerp",
  "Provider": "PostgreSQL",
  "ServerVersion": "16.1",
  "ConnectionStatus": "Connected",
  "ActiveConnections": 5,
  "MaxConnections": 100,
  "ConnectionUtilization": "5.0%"
}
```

### 3. Detailed System Health
**GET** `/api/health/detailed`

Comprehensive health check including API and database components.

```json
{
  "OverallStatus": "Healthy",
  "Timestamp": "2024-01-15T10:30:00Z",
  "Application": "CleanERP API",
  "Version": "1.0.0",
  "Environment": "Development",
  "HealthChecks": [
    {
      "Component": "API",
      "Status": "Healthy",
      "Timestamp": "2024-01-15T10:30:00Z"
    },
    {
      "Component": "Database",
      "Status": "Healthy",
      "Timestamp": "2024-01-15T10:30:00Z",
      "Details": {
        "CanConnect": true,
        "ProductCount": 0,
        "Provider": "PostgreSQL"
      }
    }
  ]
}
```

### 4. Database Connection Test
**GET** `/api/health/database/connection`

Comprehensive database connection test with performance metrics.

```json
{
  "Success": true,
  "Message": "Database connection successful",
  "ConnectionDetails": {
    "DatabaseName": "cleanerp",
    "Provider": "PostgreSQL",
    "ProductCount": 0,
    "ConnectionTime": "45ms"
  },
  "Timestamp": "2024-01-15T10:30:00Z"
}
```

### 5. Database Information
**GET** `/api/health/database/info`

Detailed database information with masked connection string.

```json
{
  "DatabaseName": "cleanerp",
  "Provider": "PostgreSQL",
  "ConnectionString": "Host=localhost;Database=cleanerp;Username=***;Password=***",
  "Tables": {
    "Products": {
      "Count": 0
    }
  },
  "SchemaManagement": "Manual",
  "MigrationsEnabled": false,
  "Timestamp": "2024-01-15T10:30:00Z"
}
```

### 6. Advanced PostgreSQL Health Check
**GET** `/api/health/database/advanced`

Advanced PostgreSQL-specific health check using direct Npgsql connections.

```json
{
  "Timestamp": "2024-01-15T10:30:00Z",
  "Status": "Healthy",
  "Message": "PostgreSQL database is healthy and accessible",
  "Database": {
    "Name": "cleanerp",
    "Server": "localhost:5432",
    "Version": "16.1.0",
    "Size": "8.5 MB",
    "Provider": "PostgreSQL"
  },
  "Connection": {
    "ActiveConnections": 3,
    "ConnectionTime": "42ms",
    "QueryTime": "8ms"
  },
  "Performance": {
    "TotalResponseTime": "42ms",
    "QueryResponseTime": "8ms",
    "Status": "Good"
  },
  "Schema": {
    "TableCount": 5,
    "ProductsTableExists": true,
    "SchemaManagement": "Manual",
    "MigrationsEnabled": false
  }
}
```

### 7. Schema Validation
**GET** `/api/health/database/schema`

Validates the existence of expected database tables for manual schema management.

```json
{
  "Success": true,
  "Message": "All expected tables exist",
  "SchemaManagement": "Manual",
  "MigrationsEnabled": false,
  "Tables": [
    {
      "TableName": "Products",
      "Exists": true,
      "RecordCount": 0,
      "Status": "OK"
    },
    {
      "TableName": "Categories",
      "Exists": false,
      "RecordCount": null,
      "Status": "Missing"
    },
    {
      "TableName": "Users",
      "Exists": true,
      "RecordCount": 1,
      "Status": "OK"
    },
    {
      "TableName": "Orders",
      "Exists": true,
      "RecordCount": 0,
      "Status": "OK"
    },
    {
      "TableName": "OrderItems",
      "Exists": true,
      "RecordCount": 0,
      "Status": "OK"
    }
  ],
  "Timestamp": "2024-01-15T10:30:00Z"
}
```

## Key Features

### Manual Schema Management
- **No EF Migrations**: The system does not use Entity Framework migrations
- **Direct PostgreSQL**: All health checks use direct PostgreSQL connections via Npgsql
- **Schema Validation**: Dedicated endpoint to validate expected table existence
- **Manual Control**: Database schema is managed manually by database administrators

### PostgreSQL-Specific Features
- **Server Version Detection**: Retrieves PostgreSQL server version
- **Connection Monitoring**: Tracks active and maximum connections
- **Database Size**: Reports database size using PostgreSQL functions
- **Performance Metrics**: Measures query and connection times
- **Direct SQL Queries**: Uses raw SQL for health checks rather than EF abstractions

### Security Features
- **Connection String Masking**: Sensitive information (passwords) are masked in responses
- **Error Handling**: Comprehensive error handling with detailed logging
- **Status Codes**: Proper HTTP status codes (200 for healthy, 503 for unhealthy, 500 for errors)

## Configuration

The database connection is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cleanerp;Username=admin;Password=your_password"
  }
}
```

## Dependencies

- **Npgsql**: Direct PostgreSQL connectivity
- **Entity Framework Core**: For basic DbContext operations (no migrations)
- **ASP.NET Core**: Web API framework

## Monitoring Recommendations

1. **Regular Health Checks**: Monitor `/api/health/detailed` for overall system health
2. **Database Performance**: Use `/api/database/health-advanced` for detailed PostgreSQL metrics
3. **Schema Validation**: Periodically check `/api/database/schema-validation` to ensure all required tables exist
4. **Connection Monitoring**: Watch connection utilization in health check responses
5. **Alert Thresholds**: Set up alerts for:
   - Database connection failures
   - High connection utilization (>80%)
   - Slow query response times (>1000ms)
   - Missing database tables

## Error Handling

All endpoints return appropriate HTTP status codes:
- **200 OK**: Healthy status
- **503 Service Unavailable**: Unhealthy database
- **500 Internal Server Error**: Configuration or connection issues

Error responses include:
- Timestamp
- Error message
- Component information
- Relevant context for troubleshooting
