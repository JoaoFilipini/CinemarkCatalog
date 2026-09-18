using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Interfaces;

namespace MovieCatalog.Infra.Messaging;

public class SqsEventProducer : IEventProducer
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqsEventProducer> _logger;

    public SqsEventProducer(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<SqsEventProducer> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string eventName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueUrl = _configuration["AWS:SqsQueueUrl"] ?? "http://localhost:4566/000000000000/film-events";
            var jsonBody = JsonSerializer.Serialize(message);

            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = jsonBody
            };

            await _sqsClient.SendMessageAsync(request, cancellationToken);
            _logger.LogInformation("Evento '{EventName}' publicado com sucesso no SQS.", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar evento '{EventName}' no SQS.", eventName);
        }
    }
}
