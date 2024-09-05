namespace ErrorHandling.Interfaces.TypeValidators;

public interface IObjectValidator<out T>
{
    T ValidateRequired(Enum errorCode);
}
