using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Infrastructure.Services;
using GoldenFiberERP.Domain.Services.Inventory;
using GoldenFiberERP.Domain.Services.Pricing;

namespace GoldenFiberERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register core infrastructure services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddHttpContextAccessor();
        services.AddTransient<ICurrentUserService, CurrentUserService>();

        // Register domain event service
        services.AddTransient<IDomainEventService, DomainEventService>();

        // Register domain services
        services.AddTransient<IStockManagementService, StockManagementService>();
        services.AddTransient<IPricingService, PricingService>();

        // Register additional services
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileService, FileService>();
        return services;
    }
}
