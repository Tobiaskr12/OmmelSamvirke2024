using ErrorHandling.Enums;
using ErrorHandling.Services.Errors;

namespace ErrorHandling.Tests.Services;

public class ErrorTranslationServiceTests
{
    private const string EnglishErrorMessage = "This is an English error message";
    private const string DanishErrorMessage = "Dette er en dansk fejlbesked";

    private enum TestErrorCode
    {
        ErrorCode,
        NonExistingErrorCode
    }

    private ErrorTranslationService _errorTranslationService;
    
    [SetUp]
    public void Setup()
    {
        _errorTranslationService = new ErrorTranslationService();
        _errorTranslationService
            .ForError(TestErrorCode.ErrorCode)
            .WithTranslation(SupportedErrorLanguage.English, EnglishErrorMessage)
            .WithTranslation(SupportedErrorLanguage.Danish, DanishErrorMessage);
    }

    [Test]
    public void GivenTheEnglishTranslationExists_WhenAttemptingToGetErrorMessageInEnglish_EnglishErrorIsReturned()
    {
        string errorMessage = _errorTranslationService.GetErrorMessage(TestErrorCode.ErrorCode, SupportedErrorLanguage.English);
        
        Assert.That(errorMessage, Is.EqualTo(EnglishErrorMessage));
    }

    [Test]
    public void GivenTheDanishTranslationExists_WhenAttemptingToGetErrorMessageInDanish_DanishErrorIsReturned()
    {
        string errorMessage = _errorTranslationService.GetErrorMessage(TestErrorCode.ErrorCode, SupportedErrorLanguage.Danish);
        
        Assert.That(errorMessage, Is.EqualTo(DanishErrorMessage));
    }

    [Test]
    public void GivenATranslationIsNotAdded_WhenAttemptingToGetErrorMessageForTheCode_FallbackValueIsReturned()
    {
        string errorMessage = _errorTranslationService.GetErrorMessage(TestErrorCode.NonExistingErrorCode, SupportedErrorLanguage.English);
        
        Assert.That(errorMessage, Is.EqualTo(ErrorTranslationService.FallBackErrorMessage));
    }

    [Test]
    public void GivenNoTranslationExist_WhenAttemptingToGetErrorMessage_FallbackValueIsReturned()
    {
        var emptyErrorTranslationService = new ErrorTranslationService();

        string errormessage = emptyErrorTranslationService.GetErrorMessage(TestErrorCode.ErrorCode, SupportedErrorLanguage.English);
        
        Assert.That(errormessage, Is.EqualTo(ErrorTranslationService.FallBackErrorMessage));
    }
}
