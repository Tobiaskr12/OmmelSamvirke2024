using ErrorHandling.Interfaces;
using ErrorHandling.Interfaces.Contracts;
using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Models;

namespace ErrorHandling.Services.Validation;

public class Validator : IValidator
{
    private readonly IErrorTranslationService _errorTranslationService;
    private readonly IErrorFactory _errorFactory;
    private readonly ErrorAggregate _errorAggregate = new();
    // private object? _validationClass;
    // private object? _currentItem;

    public Validator(IErrorTranslationService errorTranslationService, IErrorFactory errorFactory)
    {
        _errorTranslationService = errorTranslationService;
        _errorFactory = errorFactory;
    }
    
    public IClassValidatorStageTwo<T> ForClass<T>(T validationClass) where T : class
    {
        throw new NotImplementedException();
    }
    
    IStringValidator<IValueValidatorStageTwoStringType> IValueValidatorForValueMethods.ForValue(string value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoIntType> ForValue(int value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoLongType> ForValue(long value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoFloatType> ForValue(float value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoDoubleType> ForValue(double value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoDecimalType> ForValue(decimal value)
    {
        throw new NotImplementedException();
    }

    public IObjectValidator<IValueValidatorStageTwoObjectType> ForValue(object value)
    {
        throw new NotImplementedException();
    }
}
