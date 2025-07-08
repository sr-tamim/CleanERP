using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Domain.Entities.Settings;

namespace GoldenFiberERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Country> Countries { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
