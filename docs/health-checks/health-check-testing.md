# Health Check Testing Script

This script provides examples for testing all health check endpoints in the CleanERP API.

## Prerequisites

1. Ensure the API is running (typically on `http://localhost:5000` or as configured)
2. PostgreSQL database should be accessible (though some endpoints work without DB)

## Basic Health Check

```bash
# Test basic API health (no database required)
curl -X GET "http://localhost:5000/api/health" \
  -H "Accept: application/json"
```

## Database Health Checks

```bash
# Basic database health
curl -X GET "http://localhost:5000/api/health/database" \
  -H "Accept: application/json"

# Detailed system health
curl -X GET "http://localhost:5000/api/health/detailed" \
  -H "Accept: application/json"
```

## Database Connection Tests

```bash
# Test database connection
curl -X GET "http://localhost:5000/api/health/database/connection" \
  -H "Accept: application/json"

# Get database information
curl -X GET "http://localhost:5000/api/health/database/info" \
  -H "Accept: application/json"

# Advanced PostgreSQL health check
curl -X GET "http://localhost:5000/api/health/database/advanced" \
  -H "Accept: application/json"

# Schema validation for manual management
curl -X GET "http://localhost:5000/api/health/database/schema" \
  -H "Accept: application/json"
```

## PowerShell Testing (Windows)

```powershell
# Basic health check
Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method Get

# Database health check
Invoke-RestMethod -Uri "http://localhost:5000/api/health/database" -Method Get

# Advanced PostgreSQL health
Invoke-RestMethod -Uri "http://localhost:5000/api/health/database/advanced" -Method Get

# Schema validation
Invoke-RestMethod -Uri "http://localhost:5000/api/health/database/schema" -Method Get
```

## Testing with Docker

If running in Docker, replace `localhost:5000` with the appropriate container port:

```bash
# Example with Docker
curl -X GET "http://localhost:8080/api/health" \
  -H "Accept: application/json"
```

## Expected Responses

### Healthy API (no database connection)
- `/api/health` → 200 OK
- Contains: Status, Timestamp, Application, Version, Environment

### Healthy Database
- All database endpoints → 200 OK
- Contains: Connection details, performance metrics, schema info

### Unhealthy Database
- Database endpoints → 503 Service Unavailable
- Contains: Error details, timestamp, troubleshooting info

### Configuration Issues
- Endpoints → 500 Internal Server Error
- Contains: Configuration error details

## Monitoring Script

```bash
#!/bin/bash
# monitoring-health.sh

BASE_URL="http://localhost:5000"

echo "=== CleanERP Health Check Report ==="
echo "Timestamp: $(date)"
echo

# Basic API Health
echo "1. API Health:"
curl -s -w "HTTP Status: %{http_code}\n" \
  -X GET "$BASE_URL/api/health" \
  -H "Accept: application/json" | jq '.'
echo

# Database Health
echo "2. Database Health:"
curl -s -w "HTTP Status: %{http_code}\n" \
  -X GET "$BASE_URL/api/health/database" \
  -H "Accept: application/json" | jq '.'
echo

# Advanced PostgreSQL Health
echo "3. Advanced PostgreSQL Health:"
curl -s -w "HTTP Status: %{http_code}\n" \
  -X GET "$BASE_URL/api/health/database/advanced" \
  -H "Accept: application/json" | jq '.'
echo

# Schema Validation
echo "4. Schema Validation:"
curl -s -w "HTTP Status: %{http_code}\n" \
  -X GET "$BASE_URL/api/health/database/schema" \
  -H "Accept: application/json" | jq '.'
echo

echo "=== End of Health Check Report ==="
```

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Ensure API is running
   - Check port configuration
   - Verify firewall settings

2. **Database Connection Failed**
   - Verify PostgreSQL is running
   - Check connection string in appsettings.json
   - Ensure database exists
   - Verify credentials

3. **Schema Validation Failures**
   - Check if required tables exist in database
   - Verify table names match expected schema
   - Ensure proper database permissions

4. **Performance Issues**
   - Monitor connection times in health responses
   - Check PostgreSQL performance
   - Verify connection pool settings

### Health Check URLs Reference

| Endpoint | Purpose | Database Required |
|----------|---------|-------------------|
| `/api/health` | Basic API status | No |
| `/api/health/database` | Database connectivity | Yes |
| `/api/health/detailed` | Comprehensive health | Yes |
| `/api/health/database/connection` | Connection test | Yes |
| `/api/health/database/info` | Database details | Yes |
| `/api/health/database/advanced` | Advanced PostgreSQL | Yes |
| `/api/health/database/schema` | Table validation | Yes |
