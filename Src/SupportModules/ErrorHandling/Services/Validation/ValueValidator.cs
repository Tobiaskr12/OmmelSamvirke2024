using ErrorHandling.Interfaces;
using ErrorHandling.Interfaces.Contracts;
using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Models;

namespace ErrorHandling.Services.Validation;

public class ValueValidator : IValueValidator
{
    private readonly IErrorTranslationService _errorTranslationService;
    private readonly IErrorFactory _errorFactory;
    private readonly ErrorAggregate _errorAggregate;

    public ValueValidator(IErrorTranslationService errorTranslationService, IErrorFactory errorFactory, ErrorAggregate errorAggregate)
    {
        _errorTranslationService = errorTranslationService;
        _errorFactory = errorFactory;
        _errorAggregate = errorAggregate;
    }

    public IStringValidator<IValueValidatorStageTwoStringType> ForValue(string value)
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
