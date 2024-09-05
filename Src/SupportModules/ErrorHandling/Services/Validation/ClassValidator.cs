using ErrorHandling.Interfaces;
using ErrorHandling.Interfaces.Contracts;
using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Models;
using FluentResults;

namespace ErrorHandling.Services.Validation;

public class ClassValidator<T> : IClassValidator<T>, IClassValidatorStageTwo<T> where T : class
{
    private readonly IErrorTranslationService _errorTranslationService;
    private readonly IErrorFactory _errorFactory;
    private readonly ErrorAggregate _errorAggregate;

    public ClassValidator(IErrorTranslationService errorTranslationService, IErrorFactory errorFactory, ErrorAggregate errorAggregate)
    {
        _errorTranslationService = errorTranslationService;
        _errorFactory = errorFactory;
        _errorAggregate = errorAggregate;
    }

    public IClassValidatorStageTwo<T> ForClass(T validationClass)
    {
        throw new NotImplementedException();
    }
    
    public Result<TU> GetResult<TU>()
    {
        throw new NotImplementedException();
    }

    public IStringValidator<IClassValidatorStageThreeStringType<T>> ForProperty(string value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IClassValidatorStageThreeNumericType<T>> ForProperty(int value)
    {
        throw new NotImplementedException();
    }

    public INumericValidator<IClassValidatorStageThreeObjectType<T>> ForProperty(object value)
    {
        throw new NotImplementedException();
    }
}
