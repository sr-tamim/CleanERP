using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Persistence.Contexts;
using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Persistence.Seeders;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Products if none exist
        if (!await context.Products.AnyAsync())
        {
            await SeedProductsAsync(context);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        var products = new List<Product>
        {
            new Product
            {
                Name = "Cotton Fabric - White",
                Description = "High-quality white cotton fabric for textile manufacturing",
                SKU = "CTN-WHT-001",
                Price = 25.50m,
                StockQuantity = 1000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Polyester Thread - Black",
                Description = "Durable black polyester thread for sewing operations",
                SKU = "PLY-BLK-001",
                Price = 15.75m,
                StockQuantity = 500,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Silk Fabric - Blue",
                Description = "Premium blue silk fabric for luxury garments",
                SKU = "SLK-BLU-001",
                Price = 85.00m,
                StockQuantity = 250,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Denim Fabric - Indigo",
                Description = "Classic indigo denim fabric for jeans production",
                SKU = "DNM-IND-001",
                Price = 45.25m,
                StockQuantity = 750,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Wool Yarn - Gray",
                Description = "Fine gray wool yarn for knitting applications",
                SKU = "WOL-GRY-001",
                Price = 32.80m,
                StockQuantity = 300,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
    }
}
