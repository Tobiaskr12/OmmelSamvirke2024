using ErrorHandling.Interfaces.TypeValidators;

namespace ErrorHandling.Services.Validation.TypeValidators;

public class ObjectValidator<T> : IObjectValidator<T>
{
    public T ValidateRequired(Enum errorCode)
    {
        throw new NotImplementedException();
    }
}
