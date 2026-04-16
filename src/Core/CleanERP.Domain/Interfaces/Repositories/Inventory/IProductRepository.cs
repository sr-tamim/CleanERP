using CleanERP.Domain.Entities.Inventory;
using CleanERP.Domain.Interfaces.Repositories.Common;

namespace CleanERP.Domain.Interfaces.Repositories.Inventory
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    }
}
