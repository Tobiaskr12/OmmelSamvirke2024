using FluentValidation;
using FluentValidation.Results;

namespace Web.Components.Extensions;

public static class FluentValidationBlazorExtensions
{
    /// <summary>
    /// Returns a Blazor-friendly validation function that MudForm can call per field.
    /// </summary>
    public static Func<object, string, Task<IEnumerable<string>>> CreateFieldValidator<T>(this IValidator<T> validator)
    {
        return async (model, propertyName) =>
        {
            if (model is not T typedModel)
                return Array.Empty<string>();

            // Only validate the single property
            ValidationContext<T>? context = ValidationContext<T>.CreateWithOptions(
                typedModel,
                x => x.IncludeProperties(propertyName)
            );

            ValidationResult? result = await validator.ValidateAsync(context);
            return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
