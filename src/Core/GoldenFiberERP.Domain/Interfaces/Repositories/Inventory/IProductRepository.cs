using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Domain.Interfaces.Repositories.Common;

namespace GoldenFiberERP.Domain.Interfaces.Repositories.Inventory
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    }
}
