using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Interfaces.Repositories.Inventory;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Persistence.Repositories.Common;
using GoldenFiberERP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GoldenFiberERP.Persistence.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.SKU == sku, cancellationToken);
    }
}
