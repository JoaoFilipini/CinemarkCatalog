using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace NotificationConsumer.Service;

public class FilmEventPayload
{
    public string EventId { get; set; } = string.Empty;
    public string FilmId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class Worker : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Worker> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Worker(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<Worker> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = _configuration["AWS:SqsQueueUrl"] ?? throw new ArgumentNullException("SQS Queue URL missing");
        _logger.LogInformation("Iniciando escuta da fila SQS: {QueueUrl}", queueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 5
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                if (response.Messages != null)
                {
                    foreach (var message in response.Messages)
                    {
                        await ProcessMessageAsync(message, queueUrl, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagens da fila SQS.");
                await Task.Delay(3000, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(Message message, string queueUrl, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<FilmEventPayload>(message.Body, JsonOptions);

            if (payload != null)
            {
                _logger.LogInformation(
                    "Evento recebido! Evento: {EventType} | FilmeID: {FilmId} | Título: {Title} | Timestamp: {Timestamp}",
                    payload.EventType, payload.FilmId, payload.Title, payload.Timestamp);
            }
            else
            {
                _logger.LogWarning("Mensagem SQS vazia ou nula: {Body}", message.Body);
            }

            await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Mensagem inválida descartada: {Body}", message.Body);
            await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar mensagem {ReceiptHandle}", message.ReceiptHandle);
        }
    }
}
