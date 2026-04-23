using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanERP.API.Extensions;

/// <summary>
/// Operation filter to enhance Swagger documentation with module-specific metadata
/// </summary>
public class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Get controller name
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller)
            ? controller
            : "Unknown";

        // Add module information to operation description
        var moduleInfo = GetModuleInfo(controllerName);
        if (!string.IsNullOrEmpty(moduleInfo))
        {
            // Add module badge to summary
            if (!string.IsNullOrEmpty(operation.Summary))
            {
                operation.Summary = $"[{moduleInfo}] {operation.Summary}";
            }

            // Add module information to tags if not already present
            operation.Tags ??= new HashSet<OpenApiTagReference>();

            // Ensure the module tag is present
            var moduleTag = new OpenApiTagReference($"{moduleInfo} Module", null);

            if (operation.Tags.Count == 0 && !operation.Tags.Any(t => t.Name == moduleTag.Name))
            {
                operation.Tags.Add(moduleTag);
            }
        }

        // Add response examples for better documentation
        AddStandardResponses(operation);
    }

    private static string GetModuleInfo(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName)) return string.Empty;

        return controllerName.ToLowerInvariant() switch
        {
            var name when IsInventoryController(name) => "Inventory",
            var name when IsAuthController(name) => "Authentication",
            var name when IsSalesController(name) => "Sales",
            var name when IsManufacturingController(name) => "Manufacturing",
            var name when IsFinancialController(name) => "Financial",
            var name when IsSettingsController(name) => "Settings",
            var name when IsSystemController(name) => "System",
            _ => string.Empty
        };
    }

    private static void AddStandardResponses(OpenApiOperation operation)
    {
        // Add common response codes if not already present
        operation.Responses ??= new OpenApiResponses();

        // Add 400 Bad Request if not present
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - Invalid input or validation errors"
            });
        }

        // Add 401 Unauthorized for non-public endpoints
        if (!operation.Responses.ContainsKey("401") &&
            !operation.Tags?.Any(t => t.Name?.Contains("Health", StringComparison.OrdinalIgnoreCase) == true) == true)
        {
            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Unauthorized - Authentication required"
            });
        }

        // Add 500 Internal Server Error if not present
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error - Unexpected error occurred"
            });
        }
    }

    // Helper methods - duplicated from SwaggerExtensions for independence
    private static bool IsInventoryController(string controllerName)
    {
        var inventoryControllers = new[]
        {
            "products", "inventory", "stock", "warehouse", "categories", "suppliers", "purchaseorders"
        };
        return inventoryControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsAuthController(string controllerName)
    {
        var authControllers = new[]
        {
            "auth", "authentication", "authorization", "users", "roles", "permissions", "account", "identity"
        };
        return authControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSalesController(string controllerName)
    {
        var salesControllers = new[]
        {
            "sales", "orders", "customers", "quotes", "invoices", "salesreports", "customerorders", "billing"
        };
        return salesControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsManufacturingController(string controllerName)
    {
        var manufacturingControllers = new[]
        {
            "manufacturing", "production", "workorders", "billofmaterials", "qualitycontrol", "productionplanning", "machines", "processtemplates"
        };
        return manufacturingControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsFinancialController(string controllerName)
    {
        var financialControllers = new[]
        {
            "financial", "accounting", "payments", "paymentmethods", "transactions", "reports", "budgets", "costcenter", "generalledger"
        };
        return financialControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSettingsController(string controllerName)
    {
        var settingsControllers = new[]
        {
            "settings", "countries", "currencies", "timezones", "regions", "configurations", "preferences", "localization"
        };
        return settingsControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSystemController(string controllerName)
    {
        var systemControllers = new[]
        {
            "health", "system", "admin", "configuration", "monitoring", "diagnostics", "audit", "logs"
        };
        return systemControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);
    }
}
