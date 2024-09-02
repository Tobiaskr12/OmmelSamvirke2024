using OmmelSamvirke.ErrorHandling.Enums;
using OmmelSamvirke.ErrorHandling.Interfaces;
using OmmelSamvirke.ErrorHandling.Models;

namespace OmmelSamvirke.ErrorHandling.Services;

public class Validator : IValidator
{
    private readonly IErrorTranslationService _errorTranslationService;
    private readonly IErrorHandler _errorHandler;
    private readonly ErrorAggregate _errorAggregate = new();

    public Validator(IErrorTranslationService errorTranslationService, IErrorHandler errorHandler)
    {
        _errorTranslationService = errorTranslationService;
        _errorHandler = errorHandler;
    }

    public IValidator ValidateLength(string value, int minLength, int maxLength, Enum errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string nullErrorMessage = _errorTranslationService.GetErrorMessage(errorCode, SupportedErrorLanguage.Danish);
            _errorAggregate.Errors.Add(_errorHandler.CreateError(nullErrorMessage, 400));
            return this;
        }
        
        if (value.Length >= minLength && value.Length <= maxLength) return this;
        
        string errorMessage = _errorTranslationService.GetErrorMessage(errorCode, SupportedErrorLanguage.Danish);
        _errorAggregate.Errors.Add(_errorHandler.CreateError(errorMessage, 400));
        return this;
    }

    public IValidator ValidateRequired(string value, Enum errorCode)
    {
        if (!string.IsNullOrWhiteSpace(value)) return this;
        
        string errorMessage = _errorTranslationService.GetErrorMessage(errorCode, SupportedErrorLanguage.Danish);
        _errorAggregate.Errors.Add(_errorHandler.CreateError(errorMessage, 400));
        return this;
    }

    bool IValidator.IsSuccess()
    {
        return _errorAggregate.Errors.Count == 0;
    }

    int IValidator.ErrorCount()
    {
        return _errorAggregate.Errors.Count;
    }

    public List<Error> GetErrors()
    {
        return _errorAggregate.Errors;
    }
}
