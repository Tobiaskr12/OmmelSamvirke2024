using System.Text.Json;
using OmmelSamvirke2024.Api.Controllers.Util;

namespace OmmelSamvirke2024.Api.Middleware;

public class ResultExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ResultExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ResultException ex)
        {
            HandleResultException(httpContext, ex);
        }
    }

    private static void HandleResultException(HttpContext context, ResultException ex)
    {
        // Extract errors and status codes
        List<string> errors = ex.Result.Errors.Select(err => err.Message).ToList();
        List<int> statusCodes = ex.Result.Errors.SelectMany(err => err.Metadata)
            .Where(meta => meta.Key == "StatusCode")
            .Select(meta => (int)meta.Value)
            .ToList();

        int finalStatusCode = statusCodes.Any(code => code == 400) ? 400 : 500;

        // Return the error response
        context.Response.StatusCode = finalStatusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = new { Errors = errors };
        string json = JsonSerializer.Serialize(errorResponse);
        context.Response.WriteAsync(json);
    }
}
