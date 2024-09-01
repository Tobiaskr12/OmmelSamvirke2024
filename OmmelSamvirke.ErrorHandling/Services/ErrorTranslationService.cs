using OmmelSamvirke.ErrorHandling.Enums;
using OmmelSamvirke.ErrorHandling.Interfaces;

namespace OmmelSamvirke.ErrorHandling.Services;

public class ErrorTranslationService : IErrorTranslationService
{
    private readonly Dictionary<Enum, Dictionary<SupportedErrorLanguage, string>> _translations = new();
    
    public IErrorTranslationService ForError(Enum errorCode)
    {
        _translations[errorCode] = new Dictionary<SupportedErrorLanguage, string>();
        return this;
    }

    public IErrorTranslationService WithTranslation(SupportedErrorLanguage language, string message)
    {
        Enum lastErrorCode = _translations.Last().Key;
        _translations[lastErrorCode][language] = message;
        return this;
    }

    public string GetErrorMessage(Enum errorCode, SupportedErrorLanguage errorLanguage)
    {
        if (_translations.TryGetValue(errorCode, out Dictionary<SupportedErrorLanguage, string>? translation) &&
            translation.TryGetValue(errorLanguage, out string? localizedErrorMessage))
        {
            return localizedErrorMessage;
        }

        return "Unknown error";
    }
}
