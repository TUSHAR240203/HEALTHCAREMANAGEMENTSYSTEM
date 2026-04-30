using System.Net;
using System.Text.Json;

namespace HealthcareGateway.Middleware;

public sealed class GlobalExceptionMiddleware
{
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
            _logger.LogError(ex, "Unhandled gateway exception. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var payload = new
            {
                message = "An unexpected error occurred.",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
