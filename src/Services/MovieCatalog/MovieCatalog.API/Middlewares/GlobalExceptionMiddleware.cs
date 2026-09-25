using System.Net;
using System.Text.Json;
using FluentValidation;
using MovieCatalog.Domain.Exceptions;

namespace MovieCatalog.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada durante o processamento da requisição.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();
        var statusCode = HttpStatusCode.InternalServerError;
        object response;

        switch (exception)
        {
            case BusinessRuleException valEx:
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    Status = (int)statusCode,
                    Error = "Falha de Validação",
                    Errors = valEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                    CorrelationId = correlationId
                };
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    Status = (int)statusCode,
                    Error = exception.Message,
                    CorrelationId = correlationId
                };
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    Status = (int)statusCode,
                    Error = exception.Message,
                    CorrelationId = correlationId
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    Status = (int)statusCode,
                    Error = "Ocorreu um erro interno no servidor.",
                    CorrelationId = correlationId
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));    }
}
