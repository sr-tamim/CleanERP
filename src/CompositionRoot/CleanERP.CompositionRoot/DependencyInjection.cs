using CleanERP.Application;
using CleanERP.Infrastructure;
using CleanERP.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanERP.CompositionRoot;

/// <summary>
/// Main composition root for the CleanERP application.
/// This is the ONLY place where all layers are wired together.
/// Follows the Composition Root pattern for enterprise applications.
/// 
/// This project is responsible for:
/// - Registering all dependencies from all layers
/// - Managing module configuration
/// - Providing a single entry point for dependency resolution
/// - Maintaining separation between presentation and infrastructure layers
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register all application dependencies using the Composition Root pattern
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddCleanERP(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register core application layers
        services.AddCoreServices(configuration);
        
        // Register business modules
        services.AddBusinessModules(configuration);
        
        // Register cross-cutting concerns
        services.AddCrossCuttingConcerns(configuration);

        return services;
    }

    /// <summary>
    /// Register core application layers (Domain, Application, Infrastructure, Persistence)
    /// </summary>
    private static IServiceCollection AddCoreServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register layers in dependency order
        services.AddApplication();              // Application layer (no external dependencies)
        services.AddInfrastructure(configuration); // Infrastructure layer (depends on Application)
        services.AddPersistence(configuration);    // Persistence layer (depends on Application)

        return services;
    }

    /// <summary>
    /// Register business domain modules
    /// </summary>
    private static IServiceCollection AddBusinessModules(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Current modules are automatically registered through layer registrations
        // Individual modules can be conditionally registered here in the future
        
        // Future modular approach:
        // if (configuration.GetValue<bool>("Modules:Manufacturing:Enabled", false))
        // {
        //     services.AddManufacturingModule(configuration);
        // }
        // 
        // if (configuration.GetValue<bool>("Modules:Financial:Enabled", false))
        // {
        //     services.AddFinancialModule(configuration);
        // }
        // 
        // if (configuration.GetValue<bool>("Modules:HumanResources:Enabled", false))
        // {
        //     services.AddHumanResourcesModule(configuration);
        // }

        return services;
    }

    /// <summary>
    /// Register cross-cutting concerns
    /// </summary>
    private static IServiceCollection AddCrossCuttingConcerns(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Note: Health checks and caching are configured in individual layers
        // This method is reserved for future cross-cutting concerns that span multiple layers
        
        return services;
    }
}
