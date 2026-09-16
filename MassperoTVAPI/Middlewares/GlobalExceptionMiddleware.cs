using System.Net;
using System.Text.Json;
using MassperoTVAPI.Core.Helpers;

namespace MassperoTVAPI.Middlewares;

/// <summary>
/// Catches all unhandled exceptions in the request pipeline, logs full exception details
/// (including method, path, query, user, exception message, stack trace, and exact line numbers),
/// and returns a clean, structured JSON 500 ApiResponse.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";
        var user = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name
            : "Anonymous";

        // Log the complete exception with full stack trace and source line numbers
        _logger.LogError(
            ex,
            "CRITICAL: Unhandled exception during [{Method}] {Path}{QueryString} | Endpoint: {Endpoint} | User: {User} | Error: {Message}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty,
            endpoint,
            user,
            ex.Message);

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errors = new List<string> { ex.Message };
            if (_env.IsDevelopment() && ex.StackTrace != null)
            {
                errors.Add(ex.StackTrace);
            }

            var response = ApiResponse<object>.ErrorResponse(
                message: "An internal server error occurred. Please check server logs for details.",
                statusCode: (int)HttpStatusCode.InternalServerError,
                errors: errors);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
