using MovieCatalog.Infra.Data;

namespace MovieCatalog.API.HostedServices;

public class MongoIndexHostedService : IHostedService
{
    private readonly MongoIndexInitializer _initializer;
    private readonly ILogger<MongoIndexHostedService> _logger;

    public MongoIndexHostedService(MongoIndexInitializer initializer, ILogger<MongoIndexHostedService> logger)
    {
        _initializer = initializer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try { await _initializer.InitializeIndexesAsync(cancellationToken); }
        catch (Exception ex) { _logger.LogError(ex, "Falha ao criar índices do MongoDB."); }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}