using MongoDB.Driver;

namespace MovieCatalog.Infra.Data;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(string connectionString, string databaseName)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.MaxConnectionPoolSize = 100;
        settings.MinConnectionPoolSize = 10;
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

        Database = new MongoClient(settings).GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) => Database.GetCollection<T>(name);
}