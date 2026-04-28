using System.Net;
using System.Text.Json;
using FluentValidation;
using Hms.BillingApi.Common;

namespace Hms.BillingApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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

        // 🔹 Validation Errors (FluentValidation)
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");

            var errors = ex.Errors.Select(e => e.ErrorMessage);

            await WriteErrorAsync(context, HttpStatusCode.BadRequest,
                "Validation failed", errors);
        }

        // 🔹 Bad Request (custom logic errors)
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request");

            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }

        // 🔹 Conflict (business rule violations)
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict error");

            await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
        }

        // 🔹 Unexpected Errors
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await WriteErrorAsync(context,
                HttpStatusCode.InternalServerError,
                "Something went wrong. Please try again.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        object? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<string>.FailResponse(message, errors);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}