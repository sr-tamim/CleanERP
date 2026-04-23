using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Infrastructure.Services;
using CleanERP.Infrastructure.Services.Domain;
using CleanERP.Infrastructure.Services.Authentication;
using CleanERP.Infrastructure.Persistence.Repositories.Settings;
using CleanERP.Infrastructure.Persistence.Repositories.Identity;
using CleanERP.Domain.Services.Inventory;
using CleanERP.Domain.Services.Pricing;
using CleanERP.Domain.Interfaces.Repositories.Settings;
using CleanERP.Domain.Interfaces.Repositories.Identity;
using IdentityInterfaces = CleanERP.Application.Common.Interfaces.Identity;

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
        services.AddTransient<CleanERP.Application.Common.Interfaces.IAuthenticationService, AuthenticationService>();
        services.AddTransient<CleanERP.Application.Common.Interfaces.IAuthorizationService, AuthorizationService>();

        // Register additional services
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileService, FileService>();
        return services;
    }
}
