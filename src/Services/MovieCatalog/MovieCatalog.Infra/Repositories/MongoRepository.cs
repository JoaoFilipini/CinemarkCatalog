using System.Linq.Expressions;
using MongoDB.Driver;
using MovieCatalog.Domain.Entities;
using MovieCatalog.Domain.Interfaces;
using MovieCatalog.Infra.Data;

namespace MovieCatalog.Infra.Repositories;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(MongoDbContext context, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<T> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>> filter, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _collection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public virtual async Task SoftDeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(id, cancellationToken);
    }
}
