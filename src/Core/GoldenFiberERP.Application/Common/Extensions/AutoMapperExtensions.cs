using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GoldenFiberERP.Application.Common.Extensions;

/// <summary>
/// Extensions for AutoMapper configuration validation
/// </summary>
public static class AutoMapperExtensions
{
    /// <summary>
    /// Validates AutoMapper configuration in development environments
    /// Call this after service configuration to ensure all mappings are valid
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="logger">Optional logger for validation results</param>
    public static void ValidateAutoMapperConfiguration(this IServiceProvider serviceProvider, ILogger? logger = null)
    {
        try
        {
            var mapper = serviceProvider.GetRequiredService<IMapper>();
            logger?.LogInformation("Validating AutoMapper configuration...");
            
            // This will throw if there are configuration errors
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            
            logger?.LogInformation("AutoMapper configuration validation completed successfully");
        }
        catch (AutoMapperConfigurationException ex)
        {
            logger?.LogError(ex, "AutoMapper configuration validation failed: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unexpected error during AutoMapper configuration validation: {Message}", ex.Message);
            throw;
        }
    }
}
