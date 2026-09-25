using MongoDB.Driver;
using MovieCatalog.Domain.Entities;
using MovieCatalog.Domain.Interfaces;
using MovieCatalog.Infra.Data;

namespace MovieCatalog.Infra.Repositories;

public class FilmRepository : MongoRepository<Film>, IFilmRepository
{
    public FilmRepository(MongoDbContext context) : base(context, "Films")
    {
    }

    public async Task<bool> ExistsByTitleAsync(string title, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Film>.Filter.And(
            Builders<Film>.Filter.Eq(x => x.Title, title),
            Builders<Film>.Filter.Eq(x => x.IsDeleted, false));

        if (!string.IsNullOrEmpty(excludeId))
            filter = Builders<Film>.Filter.And(filter, Builders<Film>.Filter.Ne(x => x.Id, excludeId));

        return await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken) > 0;
    }

    public override async Task SoftDeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<Film>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.IsActive, false)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }
}
