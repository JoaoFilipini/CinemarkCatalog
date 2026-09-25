using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieCatalog.Domain.Entities;
using MovieCatalog.Domain.Exceptions;
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
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<T> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>> filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new BusinessRuleException("Já existe um registro com esse valor único (título).");
        }
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new BusinessRuleException("Já existe um registro com esse valor único (título).");
        }
    }

    public virtual async Task SoftDeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<T>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }
}