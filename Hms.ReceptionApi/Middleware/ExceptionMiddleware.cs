using System.Net;
using System.Text.Json;
using Hms.ReceptionApi.DTOs.Common;

namespace Hms.ReceptionApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            await Handle(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await Handle(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await Handle(context,
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    private static async Task Handle(
        HttpContext context,
        HttpStatusCode code,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var result = JsonSerializer.Serialize(
            ApiResponse<string>.Fail(message));

        await context.Response.WriteAsync(result);
    }
}