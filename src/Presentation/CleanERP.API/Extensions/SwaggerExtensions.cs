using Microsoft.OpenApi;
using System.Reflection;

namespace CleanERP.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddModularSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Register multiple OpenAPI documents, one per module
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "CleanERP API - Overview",
                Description = """
                    <p>A <strong>Clean Architecture</strong> practice project built with <strong>ASP.NET Core 10</strong>, EF Core, and PostgreSQL.</p>
                    <p>Demonstrates Domain-Driven Design, CQRS, repository pattern, JWT authentication, and modular Swagger documentation.</p>
                    <h3>Demo credentials</h3>
                    <table>
                      <tr><th>Role</th><th>Username</th><th>Password</th><th>Access</th></tr>
                      <tr><td>SuperAdmin</td><td><code>superadmin</code></td><td><code>Admin@1234</code></td><td>Full access</td></tr>
                      <tr><td>ReadOnly</td><td><code>demo</code></td><td><code>Demo@1234</code></td><td>Read-only</td></tr>
                    </table>
                    <p><em>Use the <strong>POST /api/auth/login</strong> endpoint to obtain a Bearer token, then click <strong>Authorize</strong>.</em></p>
                    """
            });

            options.SwaggerDoc("inventory", new OpenApiInfo
            {
                Version = "v1",
                Title = "Inventory Management",
                Description = "Product, stock, and inventory management APIs"
            });

            options.SwaggerDoc("auth", new OpenApiInfo
            {
                Version = "v1",
                Title = "Authentication & Authorization",
                Description = "User authentication, authorization, and security APIs"
            });

            options.SwaggerDoc("sales", new OpenApiInfo
            {
                Version = "v1",
                Title = "Sales Management",
                Description = "Order processing, customer management, and sales tracking APIs"
            });

            options.SwaggerDoc("manufacturing", new OpenApiInfo
            {
                Version = "v1",
                Title = "Manufacturing Operations",
                Description = "Production planning, work orders, and manufacturing process APIs"
            });

            options.SwaggerDoc("financial", new OpenApiInfo
            {
                Version = "v1",
                Title = "Financial Management",
                Description = "Accounting, billing, payments, and financial reporting APIs"
            });

            options.SwaggerDoc("settings", new OpenApiInfo
            {
                Version = "v1",
                Title = "Settings & Configuration",
                Description = "System settings, countries, currencies, and configuration management APIs"
            });

            options.SwaggerDoc("system", new OpenApiInfo
            {
                Version = "v1",
                Title = "System & Health",
                Description = "Health checks, system monitoring, and administrative APIs"
            });

            // Configure which controllers belong to which module
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                // Get controller name from route values
                if (!apiDesc.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerName))
                    return false;

                // For overview document, include all controllers
                if (docName == "v1")
                    return true;

                // For specific modules, only include controllers that belong to that module
                return docName switch
                {
                    "inventory" => IsInventoryController(controllerName),
                    "auth" => IsAuthController(controllerName),
                    "sales" => IsSalesController(controllerName),
                    "manufacturing" => IsManufacturingController(controllerName),
                    "financial" => IsFinancialController(controllerName),
                    "settings" => IsSettingsController(controllerName),
                    "system" => IsSystemController(controllerName),
                    _ => false
                };
            });

            // Apply operation filter for enhanced documentation
            options.OperationFilter<SwaggerOperationFilter>();

            // Add JWT Bearer authentication for Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter: **Bearer <token>**\n\nDemo logins — POST /api/auth/login:\n- `superadmin` / `Admin@1234` (full access)\n- `demo` / `Demo@1234` (read-only)",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });

            // Include XML comments if available
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseModularSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // Add each module as a separate SwaggerEndpoint
            // System & Health module first for default selection
            options.SwaggerEndpoint("/swagger/system/swagger.json", "⚙️ System & Health");
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "🏢 API Overview");
            options.SwaggerEndpoint("/swagger/inventory/swagger.json", "📦 Inventory Management");
            options.SwaggerEndpoint("/swagger/auth/swagger.json", "🔐 Authentication & Authorization");
            options.SwaggerEndpoint("/swagger/sales/swagger.json", "💰 Sales Management");
            options.SwaggerEndpoint("/swagger/manufacturing/swagger.json", "🏭 Manufacturing Operations");
            options.SwaggerEndpoint("/swagger/financial/swagger.json", "💳 Financial Management");
            options.SwaggerEndpoint("/swagger/settings/swagger.json", "⚙️ Settings & Configuration");

            options.RoutePrefix = "swagger";
            options.DocumentTitle = "CleanERP API Documentation";
            options.DefaultModelsExpandDepth(-1);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            
            // Custom CSS and JavaScript for enhanced UI
            options.InjectStylesheet("/swagger-custom.css");
            options.InjectJavascript("/swagger-custom.js");
        });

        return app;
    }

    #region Controller Module Classification

    private static bool IsInventoryController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var inventoryControllers = new[]
        {
            "Products",
            "Inventory",
            "Stock",
            "Warehouse",
            "Categories",
            "Suppliers",
            "PurchaseOrders"
        };

        return inventoryControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsAuthController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var authControllers = new[]
        {
            "Auth",
            "Authentication",
            "Authorization",
            "Users",
            "Roles",
            "Permissions",
            "Account",
            "Identity"
        };

        return authControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSalesController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var salesControllers = new[]
        {
            "Sales",
            "Orders",
            "Customers",
            "Quotes",
            "Invoices",
            "SalesReports",
            "CustomerOrders",
            "Billing"
        };

        return salesControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsManufacturingController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var manufacturingControllers = new[]
        {
            "Manufacturing",
            "Production",
            "WorkOrders",
            "BillOfMaterials",
            "QualityControl",
            "ProductionPlanning",
            "Machines",
            "ProcessTemplates"
        };

        return manufacturingControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsFinancialController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var financialControllers = new[]
        {
            "Financial",
            "Accounting",
            "Payments",
            "PaymentMethods",
            "Transactions",
            "Reports",
            "Budgets",
            "CostCenter",
            "GeneralLedger"
        };

        return financialControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSettingsController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var settingsControllers = new[]
        {
            "Settings",
            "Countries",
            "Currencies",
            "TimeZones",
            "Regions",
            "Configurations",
            "Preferences",
            "Localization"
        };

        return settingsControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSystemController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return false;
        
        var systemControllers = new[]
        {
            "Health",
            "System",
            "Admin",
            "Configuration",
            "Monitoring",
            "Diagnostics",
            "Audit",
            "Logs"
        };

        return systemControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    #endregion
}
