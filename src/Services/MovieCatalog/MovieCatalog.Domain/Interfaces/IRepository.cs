using System.Linq.Expressions;
using MovieCatalog.Domain.Entities;

namespace MovieCatalog.Domain.Interfaces;

public interface IRepository<TDocument> where TDocument : BaseEntity
{
    Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<TDocument> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<TDocument, bool>> filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task CreateAsync(TDocument entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TDocument entity, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(string id, CancellationToken cancellationToken = default);
}
