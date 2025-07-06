using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Inventory;

namespace GoldenFiberERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
