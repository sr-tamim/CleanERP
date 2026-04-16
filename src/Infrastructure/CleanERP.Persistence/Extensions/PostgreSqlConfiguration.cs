using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CleanERP.Persistence.Extensions;

/// <summary>
/// PostgreSQL-specific configurations for Entity Framework
/// </summary>
public static class PostgreSqlConfiguration
{
    /// <summary>
    /// Configure PostgreSQL-specific naming conventions and features
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void ConfigurePostgreSqlSpecifics(this ModelBuilder modelBuilder)
    {
        // Apply snake_case naming convention to all entities
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(tableName.ToSnakeCase());
            }
            
            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(columnName.ToSnakeCase());
                }
            }
            
            // Convert index names to snake_case
            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(indexName.ToSnakeCase());
                }
            }
            
            // Convert foreign key names to snake_case
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var constraintName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(constraintName))
                {
                    foreignKey.SetConstraintName(constraintName.ToSnakeCase());
                }
            }
        }
    }
    
    /// <summary>
    /// Convert PascalCase/camelCase string to snake_case
    /// </summary>
    /// <param name="text">The text to convert</param>
    /// <returns>The snake_case version of the text</returns>
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        // Insert underscore before uppercase letters that follow lowercase letters
        // and convert to lowercase
        return Regex.Replace(text, "([a-z])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}
