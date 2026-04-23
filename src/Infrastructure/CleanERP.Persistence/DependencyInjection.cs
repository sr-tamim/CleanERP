using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Domain.Interfaces.Repositories.Inventory;
using CleanERP.Domain.Interfaces.Repositories.Settings;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Repositories;
using CleanERP.Persistence.Repositories.Settings;
using CleanERP.Persistence.Services;

namespace CleanERP.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register DbContext as IUnitOfWork
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddTransient<IHealthCheckService, HealthCheckService>();

        return services;
    }
}
