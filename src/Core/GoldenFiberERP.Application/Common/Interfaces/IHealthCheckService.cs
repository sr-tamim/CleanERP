using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.Application.Common.Interfaces;

public interface IHealthCheckService
{
    Task<Result<HealthCheckResult>> GetDatabaseHealthAsync(CancellationToken cancellationToken = default);
    Task<Result<AdvancedHealthCheckResult>> GetAdvancedDatabaseHealthAsync(CancellationToken cancellationToken = default);
    Task<Result<SchemaValidationResult>> ValidateDatabaseSchemaAsync(CancellationToken cancellationToken = default);
    Task<Result<DatabaseInfoResult>> GetDatabaseInfoAsync(CancellationToken cancellationToken = default);
    Task<Result<DatabaseConnectionResult>> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default);
}

public record HealthCheckResult
{
    public string Status { get; init; } = string.Empty;
    public string Component { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string ServerVersion { get; init; } = string.Empty;
    public string ConnectionStatus { get; init; } = string.Empty;
    public int ActiveConnections { get; init; }
    public int MaxConnections { get; init; }
    public string ConnectionUtilization { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record AdvancedHealthCheckResult
{
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DatabaseDetails Database { get; init; } = new();
    public ConnectionDetails Connection { get; init; } = new();
    public PerformanceDetails Performance { get; init; } = new();
    public SchemaDetails Schema { get; init; } = new();
    public DateTime Timestamp { get; init; }
}

public record DatabaseDetails
{
    public string Name { get; init; } = string.Empty;
    public string Server { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}

public record ConnectionDetails
{
    public long ActiveConnections { get; init; }
    public string ConnectionTime { get; init; } = string.Empty;
    public string QueryTime { get; init; } = string.Empty;
}

public record PerformanceDetails
{
    public string TotalResponseTime { get; init; } = string.Empty;
    public string QueryResponseTime { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record SchemaDetails
{
    public long TableCount { get; init; }
    public bool ProductsTableExists { get; init; }
    public string SchemaManagement { get; init; } = "Manual";
    public bool MigrationsEnabled { get; init; } = false;
}

public record SchemaValidationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string SchemaManagement { get; init; } = "Manual";
    public bool MigrationsEnabled { get; init; } = false;
    public List<TableStatus> Tables { get; init; } = new();
    public DateTime Timestamp { get; init; }
}

public record TableStatus
{
    public string TableName { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public long? RecordCount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record DatabaseInfoResult
{
    public string DatabaseName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
    public Dictionary<string, object> Tables { get; init; } = new();
    public string SchemaManagement { get; init; } = "Manual";
    public bool MigrationsEnabled { get; init; } = false;
    public DateTime Timestamp { get; init; }
}

public record DatabaseConnectionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public string ConnectionTime { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
