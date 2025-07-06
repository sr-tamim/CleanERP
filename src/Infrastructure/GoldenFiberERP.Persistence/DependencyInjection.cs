using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Interfaces.Repositories.Inventory;
using GoldenFiberERP.Persistence.Contexts;
using GoldenFiberERP.Persistence.Repositories;

namespace GoldenFiberERP.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
