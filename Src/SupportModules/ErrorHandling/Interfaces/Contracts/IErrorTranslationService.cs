using ErrorHandling.Enums;

namespace ErrorHandling.Interfaces.Contracts;

public interface IErrorTranslationService
{
    IErrorTranslationService ForError(Enum errorCode);
    IErrorTranslationService WithTranslation(SupportedErrorLanguage language, string message);
    string GetErrorMessage(Enum errorCode, SupportedErrorLanguage errorLanguage);
}
