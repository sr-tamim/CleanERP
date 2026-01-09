using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Persistence.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<HealthCheckService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<HealthCheckResult>> GetDatabaseHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test basic connectivity
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return Result<HealthCheckResult>.Failure(new[] { "Cannot connect to database" });
            }

            // Get database connection info
            var connectionString = _context.Database.GetConnectionString();
            var connection = _context.Database.GetDbConnection();
            var databaseName = connection.Database;

            // Test with a direct PostgreSQL query to avoid EF dependencies
            var serverVersion = "";
            var activeConnections = 0;
            var maxConnections = 0;

            if (connection is NpgsqlConnection npgsqlConnection)
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                // Get PostgreSQL version
                using var cmd1 = connection.CreateCommand();
                cmd1.CommandText = "SELECT version();";
                var version = await cmd1.ExecuteScalarAsync(cancellationToken);
                serverVersion = version?.ToString()?.Split(' ')[1] ?? "Unknown";

                // Get connection stats
                using var cmd2 = connection.CreateCommand();
                cmd2.CommandText = @"
                    SELECT 
                        (SELECT count(*) FROM pg_stat_activity WHERE state = 'active') as active_connections,
                        (SELECT setting::int FROM pg_settings WHERE name = 'max_connections') as max_connections";
                
                using var reader = await cmd2.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync())
                {
                    activeConnections = reader.GetInt32("active_connections");
                    maxConnections = reader.GetInt32("max_connections");
                }
            }

            var result = new HealthCheckResult
            {
                Status = "Healthy",
                Component = "Database",
                DatabaseName = databaseName,
                Provider = "PostgreSQL",
                ServerVersion = serverVersion,
                ConnectionStatus = "Connected",
                ActiveConnections = activeConnections,
                MaxConnections = maxConnections,
                ConnectionUtilization = maxConnections > 0 ? $"{(double)activeConnections / maxConnections * 100:F1}%" : "N/A",
                Timestamp = DateTime.UtcNow
            };

            return Result<HealthCheckResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return Result<HealthCheckResult>.Failure(new[] { $"Database health check failed: {ex.Message}" });
        }
    }

    public async Task<Result<AdvancedHealthCheckResult>> GetAdvancedDatabaseHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                return Result<AdvancedHealthCheckResult>.Failure(new[] { "Database connection string not configured" });
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Basic connection info
            var serverVersion = connection.PostgreSqlVersion;
            var databaseName = connection.Database;
            var hostName = connection.Host;
            var port = connection.Port;

            // Test query performance
            var queryStopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var cmd = new NpgsqlCommand("SELECT 1", connection);
            await cmd.ExecuteScalarAsync(cancellationToken);
            queryStopwatch.Stop();

            // Get database size
            using var sizeCmd = new NpgsqlCommand(
                "SELECT pg_size_pretty(pg_database_size(current_database()))", connection);
            var databaseSizeResult = await sizeCmd.ExecuteScalarAsync(cancellationToken);
            var databaseSize = databaseSizeResult?.ToString() ?? "Unknown";

            // Get connection count
            using var connCmd = new NpgsqlCommand(
                "SELECT count(*) FROM pg_stat_activity WHERE datname = current_database()", connection);
            var activeConnections = (long)(await connCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);

            // Check if Products table exists (schema validation for manual management)
            using var tableCmd = new NpgsqlCommand(
                "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'products')", connection);
            var productsTableExists = (bool)(await tableCmd.ExecuteScalarAsync(cancellationToken) ?? false);

            // Get table count
            using var tableCountCmd = new NpgsqlCommand(
                "SELECT count(*) FROM pg_tables WHERE schemaname = 'public'", connection);
            var tableCount = (long)(await tableCountCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);

            stopwatch.Stop();

            var result = new AdvancedHealthCheckResult
            {
                Status = "Healthy",
                Message = "PostgreSQL database is healthy and accessible",
                Database = new DatabaseDetails
                {
                    Name = databaseName,
                    Server = $"{hostName}:{port}",
                    Version = serverVersion.ToString(),
                    Size = databaseSize,
                    Provider = "PostgreSQL"
                },
                Connection = new ConnectionDetails
                {
                    ActiveConnections = activeConnections,
                    ConnectionTime = $"{stopwatch.ElapsedMilliseconds}ms",
                    QueryTime = $"{queryStopwatch.ElapsedMilliseconds}ms"
                },
                Performance = new PerformanceDetails
                {
                    TotalResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                    QueryResponseTime = $"{queryStopwatch.ElapsedMilliseconds}ms",
                    Status = stopwatch.ElapsedMilliseconds < 1000 ? "Good" : "Slow"
                },
                Schema = new SchemaDetails
                {
                    TableCount = tableCount,
                    ProductsTableExists = productsTableExists,
                    SchemaManagement = "Manual",
                    MigrationsEnabled = false
                },
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Advanced database health check completed successfully in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);

            return Result<AdvancedHealthCheckResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Advanced database health check failed");
            return Result<AdvancedHealthCheckResult>.Failure(new[] { $"Advanced database health check failed: {ex.Message}" });
        }
    }

    public async Task<Result<SchemaValidationResult>> ValidateDatabaseSchemaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                return Result<SchemaValidationResult>.Failure(new[] { "Database connection string not configured" });
            }

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var expectedTables = new[]
            {
                "products",
                "categories",
                "users",
                "orders",
                "order_items"
            };

            var tableStatuses = new List<TableStatus>();

            foreach (var tableName in expectedTables)
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = @tableName)", 
                    connection);
                cmd.Parameters.AddWithValue("tableName", tableName);
                
                var exists = (bool)(await cmd.ExecuteScalarAsync(cancellationToken) ?? false);
                
                long? recordCount = null;
                if (exists)
                {
                    try
                    {
                        using var countCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{tableName}\"", connection);
                        recordCount = (long)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
                    }
                    catch
                    {
                        // Table exists but might have access issues
                    }
                }

                tableStatuses.Add(new TableStatus
                {
                    TableName = tableName,
                    Exists = exists,
                    RecordCount = recordCount,
                    Status = exists ? "OK" : "Missing"
                });
            }

            var allTablesExist = tableStatuses.All(t => t.Exists);

            var result = new SchemaValidationResult
            {
                Success = allTablesExist,
                Message = allTablesExist ? "All expected tables exist" : "Some tables are missing",
                SchemaManagement = "Manual",
                MigrationsEnabled = false,
                Tables = tableStatuses,
                Timestamp = DateTime.UtcNow
            };

            return Result<SchemaValidationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema validation failed");
            return Result<SchemaValidationResult>.Failure(new[] { $"Schema validation failed: {ex.Message}" });
        }
    }

    public async Task<Result<DatabaseInfoResult>> GetDatabaseInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseName = _context.Database.GetDbConnection().Database;
            var connectionString = _context.Database.GetConnectionString();
            
            // Mask sensitive information in connection string
            var maskedConnectionString = MaskConnectionString(connectionString);
            
            // Get table information using the repository pattern through context
            var productCount = await _context.Products.CountAsync(cancellationToken);
            
            var result = new DatabaseInfoResult
            {
                DatabaseName = databaseName,
                Provider = "PostgreSQL",
                ConnectionString = maskedConnectionString,
                Tables = new Dictionary<string, object>
                {
                    { "Products", new { Count = productCount } }
                },
                SchemaManagement = "Manual",
                MigrationsEnabled = false,
                Timestamp = DateTime.UtcNow
            };

            return Result<DatabaseInfoResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database information");
            return Result<DatabaseInfoResult>.Failure(new[] { $"Failed to get database information: {ex.Message}" });
        }
    }

    public async Task<Result<DatabaseConnectionResult>> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test basic connectivity
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return Result<DatabaseConnectionResult>.Failure(new[] { "Cannot connect to database" });
            }

            // Get connection details
            var connectionString = _context.Database.GetConnectionString();
            var databaseName = _context.Database.GetDbConnection().Database;
            
            // Test a simple query
            var productCount = await _context.Products.CountAsync(cancellationToken);
            
            stopwatch.Stop();

            var result = new DatabaseConnectionResult
            {
                Success = true,
                Message = "Database connection successful",
                DatabaseName = databaseName,
                Provider = "PostgreSQL",
                ProductCount = productCount,
                ConnectionTime = $"{stopwatch.ElapsedMilliseconds}ms",
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Database connection test successful in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return Result<DatabaseConnectionResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return Result<DatabaseConnectionResult>.Failure(new[] { $"Database connection failed: {ex.Message}" });
        }
    }

    private static string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "Not Available";

        // Simple masking - replace password value
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"(Password|Pwd)=([^;]*)", 
            "$1=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
