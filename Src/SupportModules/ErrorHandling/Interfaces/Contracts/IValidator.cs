namespace ErrorHandling.Interfaces.Contracts;

public interface IValidator : IValueValidator
{
    IClassValidatorStageTwo<T> ForClass<T>(T validationClass) where T : class;
}
