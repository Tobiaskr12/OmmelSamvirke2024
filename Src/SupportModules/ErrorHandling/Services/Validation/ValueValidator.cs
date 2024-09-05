using ErrorHandling.Interfaces;
using ErrorHandling.Interfaces.Contracts;
using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Models;
using FluentResults;

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

    public IValueValidator ValidateLength(int minLength, int maxLength, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public IValueValidator ValidateRange(int min, int max, Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public IValueValidator ValidateRequired(Enum errorCode)
    {
        throw new NotImplementedException();
    }

    public Result<TU> GetResult<TU>()
    {
        throw new NotImplementedException();
    }

    public IStringValidator<IValueValidatorStageTwoStringType> ForValue(string value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoNumericType> ForValue(int value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IValueValidatorStageTwoObjectType> ForValue(object value)
    {
        throw new NotImplementedException();
    }
}
