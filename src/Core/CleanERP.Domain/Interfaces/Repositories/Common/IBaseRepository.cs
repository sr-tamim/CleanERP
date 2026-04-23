using CleanERP.Domain.Entities.Common;
using CleanERP.Domain.Specifications;

namespace CleanERP.Domain.Interfaces.Repositories.Common
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

        // Specification methods
        Task<IEnumerable<T>> GetBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<T?> GetFirstBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<int> CountBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    }
}
