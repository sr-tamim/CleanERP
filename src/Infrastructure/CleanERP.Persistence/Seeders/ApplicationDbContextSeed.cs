using Microsoft.EntityFrameworkCore;
using CleanERP.Persistence.Contexts;
using CleanERP.Domain.Entities.Inventory;

namespace CleanERP.Persistence.Seeders;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Products if none exist
        if (!await context.Products.AnyAsync())
        {
            SeedProductsAsync(context);
        }

        await context.SaveChangesAsync();
    }

    private static void SeedProductsAsync(ApplicationDbContext context)
    {
        var products = new List<Product>
        {
            Product.Create(
                name: "Cotton Fabric - White",
                description: "High-quality white cotton fabric for textile manufacturing",
                sku: "CTN-WHT-001",
                category: "Fabric",
                price: 25.50m,
                cost: 20.00m,
                initialStock: 1000,
                minimumStockLevel: 100,
                reorderLevel: 200,
                unit: "Meters",
                createdBy: 0),
            Product.Create(
                name: "Polyester Thread - Black",
                description: "Durable black polyester thread for sewing operations",
                sku: "PLY-BLK-001",
                category: "Thread",
                price: 15.75m,
                cost: 12.00m,
                initialStock: 500,
                minimumStockLevel: 50,
                reorderLevel: 100,
                unit: "Spools",
                createdBy: 0),
            Product.Create(
                name: "Silk Fabric - Blue",
                description: "Premium blue silk fabric for luxury garments",
                sku: "SLK-BLU-001",
                category: "Fabric",
                price: 85.00m,
                cost: 70.00m,
                initialStock: 250,
                minimumStockLevel: 25,
                reorderLevel: 50,
                unit: "Meters",
                createdBy: 0),
            Product.Create(
                name: "Denim Fabric - Indigo",
                description: "Classic indigo denim fabric for jeans production",
                sku: "DNM-IND-001",
                category: "Fabric",
                price: 45.25m,
                cost: 35.00m,
                initialStock: 750,
                minimumStockLevel: 75,
                reorderLevel: 150,
                unit: "Meters",
                createdBy: 0),
            Product.Create(
                name: "Wool Yarn - Gray",
                description: "Fine gray wool yarn for knitting applications",
                sku: "WOL-GRY-001",
                category: "Yarn",
                price: 32.80m,
                cost: 25.00m,
                initialStock: 300,
                minimumStockLevel: 30,
                reorderLevel: 60,
                unit: "Balls",
                createdBy: 0)
        };

        context.Products.AddRange(products);
    }
}
