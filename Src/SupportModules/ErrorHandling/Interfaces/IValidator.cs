using FluentResults;

namespace ErrorHandling.Interfaces;

public interface IValidator
{
    IGenericValidator ForClass<T>(T validationClass);
    IGenericValidator ForValue<T>(T value);
    public bool IsSuccess();
    public int ErrorCount();
    public List<Models.Error> GetErrors();
}

public interface IGenericValidator : IStringValidators
{
    IGenericValidator ForProperty<T>(T value);
    IGenericValidator ValidateRequired(Enum errorCode);
    Result<T> GetResult<T>();
};

public interface IStringValidators
{
    IGenericValidator ValidateLength(int minLength, int maxLength, Enum errorCode);
}
