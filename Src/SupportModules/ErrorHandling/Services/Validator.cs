using ErrorHandling.Enums;
using ErrorHandling.Interfaces;
using ErrorHandling.Models;
using FluentResults;
using Error = FluentResults.Error;

namespace ErrorHandling.Services;

public class Validator : IValidator, IGenericValidator
{
    private readonly IErrorTranslationService _errorTranslationService;
    private readonly IErrorHandler _errorHandler;
    private readonly ErrorAggregate _errorAggregate = new();
    private object? _validationClass;
    private object? _currentItem;

    public Validator(IErrorTranslationService errorTranslationService, IErrorHandler errorHandler)
    {
        _errorTranslationService = errorTranslationService;
        _errorHandler = errorHandler;
    }

    public IGenericValidator ForClass<T>(T validationClass)
    {
        _validationClass = validationClass;
        _errorAggregate.Errors.Clear();
        return this;
    }

    public IGenericValidator ForValue<T>(T value)
    {
        _currentItem = value;
        if (_validationClass is null)
        {
            _errorAggregate.Errors.Clear();
        }
        return this;
    }
    
    public IGenericValidator ForProperty<T>(T value)
    {
        return ForValue(value);
    }
    
    public IGenericValidator ValidateLength(int minLength, int maxLength, Enum errorCode)
    {
        ValidateCurrentItemNotNull();
        if (_currentItem is not string value)
        {
            throw new InvalidOperationException("Current item is not a string.");
        }

        if (value.Length >= minLength && value.Length <= maxLength) return this;

        string errorMessage = _errorTranslationService.GetErrorMessage(errorCode, SupportedErrorLanguage.Danish);
        _errorAggregate.Errors.Add(_errorHandler.CreateError(errorMessage, 400));
        return this;
    }
    
    public IGenericValidator ValidateRequired(Enum errorCode)
    {
        ValidateCurrentItemNotNull();
        if (_currentItem != null) return this;
        
        string errorMessage = _errorTranslationService.GetErrorMessage(errorCode, SupportedErrorLanguage.Danish);
        _errorAggregate.Errors.Add(_errorHandler.CreateError(errorMessage, 400));
        return this;
    }
    
    public Result<T> GetResult<T>()
    {
        try
        {
            if (IsSuccess())
            {
                // If validating properties of a class
                if (_validationClass is not null)
                {
                    return Result.Ok((T)_validationClass);
                }
                
                // If validating simpler values
                return _currentItem is null ? 
                    Result.Fail(_errorTranslationService.GetErrorMessage(GenericErrorCode.GenericValidationError, SupportedErrorLanguage.Danish)) : 
                    Result.Ok((T)_currentItem);
            }

            // Extract errors and attach status code metadata
            IEnumerable<IError> errors = GetErrors().Select(error =>
                new Error(error.Message).WithMetadata("StatusCode", error.StatusCode));
            
            return Result.Fail(errors);
        }
        finally
        {
            Reset();
        }
    }

    public bool IsSuccess()
    {
        return _errorAggregate.Errors.Count == 0;
    }

    public int ErrorCount()
    {
        return _errorAggregate.Errors.Count;
    }

    public List<Models.Error> GetErrors()
    {
        return _errorAggregate.Errors;
    }

    private void ValidateCurrentItemNotNull()
    {
        if (_currentItem != null) return;
        
        string errorMessage = _errorTranslationService.GetErrorMessage(GenericErrorCode.ValidatorItemNotSet, SupportedErrorLanguage.Danish);
        _errorAggregate.Errors.Add(_errorHandler.CreateError(errorMessage, 400));
    }

    private void Reset()
    {
        _currentItem = null;
        _validationClass = null;
    }
}
