using System.Net;

namespace GameServer.Api.Middleware;

public sealed class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var expectedApiKey = _configuration["ApiKey"];
        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync("API key is not configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (!string.Equals(providedApiKey, expectedApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        await _next(context);
    }
}
