using MongoDB.Driver;
using MovieCatalog.Domain.Entities;

namespace MovieCatalog.Infra.Data;

public class MongoIndexInitializer
{
    private readonly MongoDbContext _context;

    public MongoIndexInitializer(MongoDbContext context)
    {
        _context = context;
    }

    public async Task InitializeIndexesAsync(CancellationToken cancellationToken = default)
    {
        var collection = _context.GetCollection<Film>("Films");

        var titleIndexKeys = Builders<Film>.IndexKeys.Ascending(x => x.Title);
        var titleIndexOptions = new CreateIndexOptions<Film>
        {
            Unique = true,
            PartialFilterExpression = Builders<Film>.Filter.Eq(x => x.IsDeleted, false)
        };
        var titleIndexModel = new CreateIndexModel<Film>(titleIndexKeys, titleIndexOptions);

        
        var genreActiveKeys = Builders<Film>.IndexKeys.Combine(
            Builders<Film>.IndexKeys.Ascending(x => x.Genre),
            Builders<Film>.IndexKeys.Ascending(x => x.IsActive)
        );
        var genreActiveModel = new CreateIndexModel<Film>(genreActiveKeys);

        await collection.Indexes.CreateManyAsync([titleIndexModel, genreActiveModel], cancellationToken);
    }
}
