using System.Text.Json;
using OmmelSamvirke.SupportModules.MediatRConfig.Exceptions;

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

        // Return the error response
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new { Errors = errors };
        string json = JsonSerializer.Serialize(errorResponse);
        context.Response.WriteAsync(json);
    }
}
