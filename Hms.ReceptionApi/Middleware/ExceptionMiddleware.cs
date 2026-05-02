using FluentValidation;
using Hms.ReceptionApi.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace Hms.ReceptionApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var errors = ex.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            await WriteErrorAsync(
                context,
                HttpStatusCode.BadRequest,
                "Validation failed.",
                errors);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var errors = _environment.IsDevelopment()
                ? new List<string> { ex.ToString() }
                : null;

            await WriteErrorAsync(
                context,
                HttpStatusCode.Conflict,
                ex.Message,
                errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var message = _environment.IsDevelopment()
                ? ex.Message
                : "Internal server error";

            var errors = _environment.IsDevelopment()
                ? new List<string>
                {
                    ex.GetType().Name,
                    ex.ToString()
                }
                : null;

            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                message,
                errors);
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        List<string>? errors = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = ApiResponse<object>.Fail(
            message,
            errors
        );

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}