using EmailWrapper.Errors;
using FluentResults;
using OmmelSamvirke.ErrorHandling.Interfaces;

namespace EmailWrapper.Validators;

public static class ContactListValidator
{
    public static Result ValidateContactList(string name, string description, IValidator validator)
    {
        validator
            .ValidateLength(name, 3, 200, ContactListErrors.Enums.InvalidNameLength)
            .ValidateLength(description, 5, 2000, ContactListErrors.Enums.InvalidDescriptionLength);

        return validator.IsSuccess()
            ? Result.Ok()
            : Result.Fail("Bah");
    }
}