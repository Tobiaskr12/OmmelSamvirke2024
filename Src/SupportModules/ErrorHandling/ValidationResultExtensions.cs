using FluentResults;
using FluentValidation.Results;

namespace ErrorHandling;

public static class ValidationResultExtensions
{
    public static Result<T> GetResult<T>(this ValidationResult validationResult, T validatedObject)
    {
        if (validationResult.IsValid) return Result.Ok(validatedObject);
        var errors = new List<IError>();
        
        List<ValidationFailure>? validationErrors = validationResult.Errors;

        foreach (ValidationFailure validationError in validationErrors)
        {
            errors.Add(
                new Error(validationError.ErrorMessage)
                    .WithMetadata("StatusCode", 400)
            );
        }

        return Result.Fail(errors);
    }
}
