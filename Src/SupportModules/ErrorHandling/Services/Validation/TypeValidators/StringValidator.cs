using ErrorHandling.Interfaces.TypeValidators;

namespace ErrorHandling.Services.Validation.TypeValidators;

public class StringValidator<T> : IStringValidator<T>
{
    public T ValidateLength(int minLength, int maxLength, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public T ValidateRequired(Enum errorCode)
    {
        throw new NotImplementedException();
    }
}
