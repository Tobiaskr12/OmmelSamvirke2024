namespace ErrorHandling.Interfaces.TypeValidators;

public interface INumericValidator<out T>
{
    T ValidateRange(int min, int max, Enum errorCode);
    T ValidateRange(long min, long max, Enum errorCode);
    T ValidateRange(float min, float max, Enum errorCode);
    T ValidateRange(double min, double max, Enum errorCode);
    T ValidateRange(decimal min, decimal max, Enum errorCode);
}
