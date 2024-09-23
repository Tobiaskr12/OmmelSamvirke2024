using System.Text.Json;
using FluentValidation;

namespace OmmelSamvirke2024.Api.Middleware;

public class ValidationExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException ex)
        {
            HandleValidationException(httpContext, ex);
        }
    }

    private static void HandleValidationException(HttpContext context, ValidationException ex)
    {
        // Extract errors and status codes
        IEnumerable<string> errorMessages = ex.Errors.Select(error => error.ErrorMessage);

        // Return the error response
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var errorResponse = new { Errors = errorMessages };
        string json = JsonSerializer.Serialize(errorResponse);
        context.Response.WriteAsync(json);
    }
}
