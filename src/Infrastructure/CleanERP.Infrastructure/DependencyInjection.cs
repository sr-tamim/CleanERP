using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Infrastructure.Services;
using CleanERP.Domain.Services.Inventory;
using CleanERP.Domain.Services.Pricing;
using CleanERP.Infrastructure.Services.Domain;

namespace CleanERP.Infrastructure;

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
