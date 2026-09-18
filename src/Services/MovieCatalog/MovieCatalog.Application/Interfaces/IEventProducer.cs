namespace MovieCatalog.Application.Interfaces;

public interface IEventProducer
{
    Task PublishAsync<T>(string eventName, T message, CancellationToken cancellationToken = default);
}
