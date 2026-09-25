using System.Text.Json.Serialization;
using Amazon.SQS;
using FluentValidation;
using MovieCatalog.API.HostedServices;
using MovieCatalog.API.Middlewares;
using MovieCatalog.Application.Configurations;
using MovieCatalog.Application.DTOs;
using MovieCatalog.Application.Interfaces;
using MovieCatalog.Application.Services;
using MovieCatalog.Application.Validators;
using MovieCatalog.Domain.Interfaces;
using MovieCatalog.Infra.Caching;
using MovieCatalog.Infra.Data;
using MovieCatalog.Infra.Messaging;
using MovieCatalog.Infra.Repositories;
using Serilog;
using Serilog.Formatting.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter()));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
var mongoConn = builder.Configuration["MongoDb:ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDbName = builder.Configuration["MongoDb:DatabaseName"] ?? "CinemarkCatalogDb";
builder.Services.AddSingleton(new MongoDbContext(mongoConn, mongoDbName));
builder.Services.AddSingleton<MongoIndexInitializer>();
builder.Services.AddHostedService<MongoIndexHostedService>();
builder.Services.AddScoped<IFilmRepository, FilmRepository>();

// Redis
var redisOptions = ConfigurationOptions.Parse(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379");
redisOptions.AbortOnConnectFail = false;
redisOptions.ConnectTimeout = 2000;
redisOptions.SyncTimeout = 500;
redisOptions.AsyncTimeout = 500;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

// AWS SQS / LocalStack
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
builder.Services.AddScoped<IEventProducer, SqsEventProducer>();

// Application
builder.Services.AddScoped<IValidator<CreateFilmInput>, CreateFilmInputValidator>();
builder.Services.AddScoped<IValidator<UpdateFilmInput>, UpdateFilmInputValidator>();
builder.Services.AddScoped<IFilmAppService, FilmAppService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();