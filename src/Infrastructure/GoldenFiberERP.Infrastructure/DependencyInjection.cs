using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Infrastructure.Services;
using GoldenFiberERP.Infrastructure.Services.Domain;
using GoldenFiberERP.Infrastructure.Services.Authentication;
using GoldenFiberERP.Infrastructure.Persistence.Repositories.Settings;
using GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;
using GoldenFiberERP.Domain.Services.Inventory;
using GoldenFiberERP.Domain.Services.Pricing;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using IdentityInterfaces = GoldenFiberERP.Application.Common.Interfaces.Identity;

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

        // Register repositories
        services.AddTransient<ICountryRepository, CountryRepository>();

        // Register identity repositories
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IRoleRepository, RoleRepository>();
        services.AddTransient<IPermissionRepository, PermissionRepository>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddTransient<IUserRoleRepository, UserRoleRepository>();
        services.AddTransient<IUserPermissionRepository, UserPermissionRepository>();
        services.AddTransient<IRolePermissionRepository, RolePermissionRepository>();

        // Register authentication services
        services.AddTransient<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<GoldenFiberERP.Application.Common.Interfaces.IAuthenticationService, AuthenticationService>();
        services.AddTransient<GoldenFiberERP.Application.Common.Interfaces.IAuthorizationService, AuthorizationService>();

        // Register additional services
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IHealthCheckService, HealthCheckService>();

        return services;
    }
}
