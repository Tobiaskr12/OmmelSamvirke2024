using ErrorHandling.Interfaces;
using FluentResults;

namespace ErrorHandling.Helpers;

public static class ValidationHelper
{
    public static Result<T> GetValidationResult<T>(T valueToValidate, IValidator validator)
    {
        if (validator.IsSuccess())
        {
            return Result.Ok(valueToValidate);
        }

        // Extract errors and attach status code metadata
        IEnumerable<IError> errors = validator
            .GetErrors()
            .Select(error => new Error(error.Message).WithMetadata("StatusCode", error.StatusCode));

        return Result.Fail(errors);
    }
}
