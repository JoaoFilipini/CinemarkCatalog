using Amazon.SQS;
using FluentValidation;
using MovieCatalog.API.Middlewares;
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

// Configuração do Serilog em formato JSON
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infraestrutura - MongoDB
var mongoConn = builder.Configuration["MongoDb:ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDbName = builder.Configuration["MongoDb:DatabaseName"] ?? "CinemarkCatalogDb";
builder.Services.AddSingleton(new MongoDbContext(mongoConn, mongoDbName));
builder.Services.AddScoped<IFilmRepository, FilmRepository>();

// Infraestrutura - Redis Cache
var redisConn = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Infraestrutura - AWS SQS / LocalStack
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

// Aplicação - FluentValidation e Services
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
