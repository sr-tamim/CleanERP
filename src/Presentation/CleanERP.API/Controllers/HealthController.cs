using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanERP.Application.Features.Health.Queries;

namespace CleanERP.API.Controllers;

/// <summary>
/// System health monitoring and diagnostic endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("System Health")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IMediator mediator, ILogger<HealthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Basic API health check
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Application = "CleanERP API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }

    /// <summary>
    /// Database connection health check
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        try
        {
            var result = await _mediator.Send(new GetDatabaseHealthQuery());
            
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Component = "Database",
                Errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Component = "Database",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Advanced PostgreSQL health check with detailed metrics
    /// </summary>
    [HttpGet("database/advanced")]
    public async Task<IActionResult> GetAdvancedDatabaseHealth()
    {
        try
        {
            var result = await _mediator.Send(new GetAdvancedDatabaseHealthQuery());
            
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            
            return StatusCode(500, new
            {
                Timestamp = DateTime.UtcNow,
                Status = "Unhealthy",
                Message = "Advanced database health check failed",
                Errors = result.Errors,
                Schema = new
                {
                    SchemaManagement = "Manual",
                    MigrationsEnabled = false
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Advanced database health check failed");
            return StatusCode(500, new
            {
                Timestamp = DateTime.UtcNow,
                Status = "Unhealthy",
                Message = "Advanced database health check failed",
                Error = ex.Message,
                Schema = new
                {
                    SchemaManagement = "Manual",
                    MigrationsEnabled = false
                }
            });
        }
    }

    /// <summary>
    /// Database connection test with performance metrics
    /// </summary>
    [HttpGet("database/connection")]
    public async Task<IActionResult> TestDatabaseConnection()
    {
        try
        {
            var result = await _mediator.Send(new GetDatabaseHealthQuery());
            
            if (result.Succeeded)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Database connection successful",
                    ConnectionDetails = new
                    {
                        result.Data.DatabaseName,
                        result.Data.Provider,
                        ConnectionTime = "N/A", // This would need to be added to the service
                        result.Data.Timestamp
                    },
                    Timestamp = result.Data.Timestamp
                });
            }
            
            return BadRequest(new
            {
                Success = false,
                Message = "Cannot connect to database",
                Errors = result.Errors,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Database connection failed",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get database information with masked credentials
    /// </summary>
    [HttpGet("database/info")]
    public async Task<IActionResult> GetDatabaseInfo()
    {
        try
        {
            var result = await _mediator.Send(new GetDatabaseInfoQuery());
            
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            
            return StatusCode(500, new
            {
                Success = false,
                Message = "Failed to get database information",
                Errors = result.Errors,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database information");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Failed to get database information",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Validate database schema for manual management
    /// </summary>
    [HttpGet("database/schema")]
    public async Task<IActionResult> ValidateDatabaseSchema()
    {
        try
        {
            var result = await _mediator.Send(new ValidateDatabaseSchemaQuery());
            
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            
            return StatusCode(500, new
            {
                Success = false,
                Message = "Schema validation failed",
                Errors = result.Errors,
                SchemaManagement = "Manual",
                MigrationsEnabled = false,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema validation failed");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Schema validation failed",
                Error = ex.Message,
                SchemaManagement = "Manual",
                MigrationsEnabled = false,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Detailed system health check
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var healthChecks = new List<object>();

        // API Health
        healthChecks.Add(new
        {
            Component = "API",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });

        // Database Health
        try
        {
            var dbHealthResult = await _mediator.Send(new GetDatabaseHealthQuery());
            
            if (dbHealthResult.Succeeded)
            {
                healthChecks.Add(new
                {
                    Component = "Database",
                    Status = "Healthy",
                    Timestamp = dbHealthResult.Data.Timestamp,
                    Details = new
                    {
                        CanConnect = true,
                        dbHealthResult.Data.DatabaseName,
                        dbHealthResult.Data.Provider,
                        dbHealthResult.Data.ServerVersion
                    }
                });
            }
            else
            {
                healthChecks.Add(new
                {
                    Component = "Database",
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Errors = dbHealthResult.Errors
                });
            }
        }
        catch (Exception ex)
        {
            healthChecks.Add(new
            {
                Component = "Database",
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }

        // Overall status
        var overallStatus = healthChecks.All(h => h.GetType().GetProperty("Status")?.GetValue(h)?.ToString() == "Healthy") 
            ? "Healthy" : "Unhealthy";

        return Ok(new
        {
            OverallStatus = overallStatus,
            Timestamp = DateTime.UtcNow,
            Application = "CleanERP API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            HealthChecks = healthChecks
        });
    }
}
