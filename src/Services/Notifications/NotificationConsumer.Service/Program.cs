using Amazon.SQS;
using NotificationConsumer.Service;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

var awsOptions = builder.Configuration.GetSection("AWS");
builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var config = new AmazonSQSConfig
    {
        ServiceURL = awsOptions["ServiceUrl"] ?? "http://localhost:4566",
        AuthenticationRegion = awsOptions["Region"] ?? "us-east-1"
    };
    return new AmazonSQSClient("test", "test", config);
});

builder.Services.AddHostedService<Worker>();
builder.Services.AddHealthChecks().AddCheck<SqsHealthCheck>("SQS");

var app = builder.Build();
app.MapHealthChecks("/health");
app.Run();