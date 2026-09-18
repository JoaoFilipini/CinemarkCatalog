using System.Text.Json;
using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Interfaces;
using StackExchange.Redis;

namespace MovieCatalog.Infra.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao obter chave '{Key}' do Redis. Fallback ativado para buscar diretamente no banco.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao salvar chave '{Key}' no Redis. Operação prosseguirá sem cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao remover chave '{Key}' do Redis.", key);
        }
    }

    // RedisCacheService.cs
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
                await db.KeyDeleteAsync(key);
            }
    }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao remover chaves com prefixo '{Prefix}' do Redis.", prefix);
        }
}
}
