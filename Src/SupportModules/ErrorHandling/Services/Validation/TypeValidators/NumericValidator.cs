using ErrorHandling.Interfaces.TypeValidators;

namespace ErrorHandling.Services.Validation.TypeValidators;

public class NumericValidator<T> : INumericValidator<T>
{
    public T ValidateRange(int min, int max, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public T ValidateRange(long min, long max, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public T ValidateRange(short min, short max, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public T ValidateRange(float min, float max, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public T ValidateRange(double min, double max, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public T ValidateRange(decimal min, decimal max, Enum errorCode)
    {
        throw new NotImplementedException();
    }
}
