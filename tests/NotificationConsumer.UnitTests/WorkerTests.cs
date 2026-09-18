using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationConsumer.Service;
using Xunit;

namespace NotificationConsumer.UnitTests;

public class WorkerTests
{
    private readonly Mock<IAmazonSQS> _sqsClientMock = new();
    private readonly Mock<ILogger<Worker>> _loggerMock = new();

    [Fact]
    public async Task Worker_ShouldReceiveAndDeleteMessageFromSQS()
    {
        // Configuração em memória robusta
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"AWS:SqsQueueUrl", "http://localhost:4566/000000000000/film-events"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var message = new Message
        {
            Body = "{\"Event\":\"FilmCreated\",\"Data\":{\"Id\":\"123\",\"Title\":\"Inception\"}}",
            ReceiptHandle = "handle-123"
        };

        var receiveResponse = new ReceiveMessageResponse
        {
            Messages = new List<Message> { message }
        };

        // Suporte para chamadas via Request ou via parâmetros diretos
        _sqsClientMock
            .Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiveResponse);

        _sqsClientMock
            .Setup(x => x.ReceiveMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiveResponse);

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse());

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(It.IsAny<DeleteMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse());

        var worker = new Worker(_sqsClientMock.Object, configuration, _loggerMock.Object);

        using var cts = new CancellationTokenSource();

        var startTask = worker.StartAsync(cts.Token);
        await Task.Delay(300);

        cts.Cancel();
        await worker.StopAsync(CancellationToken.None);
        await startTask;

        // Verifica a remoção da mensagem na fila
        try
        {
            _sqsClientMock.Verify(
                x => x.DeleteMessageAsync(It.IsAny<string>(), "handle-123", It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );
        }
        catch
        {
            _sqsClientMock.Verify(
                x => x.DeleteMessageAsync(It.Is<DeleteMessageRequest>(r => r.ReceiptHandle == "handle-123"), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );
        }
    }
}
