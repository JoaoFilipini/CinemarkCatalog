using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MovieCatalog.Infra.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    // Construtor padrão com IConfiguration
    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb") 
            ?? configuration["MongoDb:ConnectionString"] 
            ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDb:DatabaseName"] 
            ?? "MovieCatalogDb";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    // Construtor recebendo string de conexão e nome do banco diretamente (usado no Program.cs atual)
    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    // Construtor recebendo apenas a string de conexão (caso o Program.cs passe só ela)
    public MongoDbContext(string connectionString)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("MovieCatalogDb");
    }

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
