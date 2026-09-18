using Amazon.SQS;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotificationConsumer.Service;

public class SqsHealthCheck : IHealthCheck
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;

    public SqsHealthCheck(IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueUrl = _configuration["AWS:SqsQueueUrl"];
            var attributes = await _sqsClient.GetQueueAttributesAsync(queueUrl, new List<string> { "ApproximateNumberOfMessages" }, cancellationToken);
            return attributes.HttpStatusCode == System.Net.HttpStatusCode.OK
                ? HealthCheckResult.Healthy("Conexão com SQS operacional.")
                : HealthCheckResult.Unhealthy("Resposta inválida do SQS.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha na conexão com a fila SQS.", ex);
        }
    }
}
