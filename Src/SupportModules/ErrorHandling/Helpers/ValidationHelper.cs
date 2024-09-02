using ErrorHandling.Interfaces;
using FluentResults;

namespace ErrorHandling.Helpers;

public static class ValidationHelper
{
    public static Result<T> ValidateAndReturnResult<T>(T value, IValidator validator)
    {
        if (validator.IsSuccess())
        {
            return Result.Ok(value);
        }

        // Extract errors and attach status code metadata
        IEnumerable<IError> errors = validator
            .GetErrors()
            .Select(error => new Error(error.Message).WithMetadata("StatusCode", error.StatusCode));

        return Result.Fail(errors);
    }
}
