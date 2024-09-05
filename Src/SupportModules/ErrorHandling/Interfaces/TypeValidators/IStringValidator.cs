namespace ErrorHandling.Interfaces.TypeValidators;

public interface IStringValidator<out T> : IObjectValidator<T>
{
    T ValidateLength(int minLength, int maxLength, Enum errorCode);
}
