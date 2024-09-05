using ErrorHandling.Enums;
using ErrorHandling.Interfaces.Contracts;
using ErrorHandling.Services.Validation;
using FluentResults;
using NSubstitute;
using Error = ErrorHandling.Models.Error;

namespace ErrorHandling.Tests.Services;

public class ValidatorTests
{
    private IErrorTranslationService _errorTranslationService;
    private IErrorFactory _errorFactory;
    private IValidator _validator;

    private Error _testError;
    private string _testErrorMessage;
    private int _testErrorStatusCode;
    private string _testErrorMessageTranslatedMessage;

    private enum TestErrorCodes
    {
        InvalidLength,
        InvalidRange
    }
    
    [SetUp]
    public void Setup()
    {
        _errorTranslationService = Substitute.For<IErrorTranslationService>();
        _errorFactory = Substitute.For<IErrorFactory>();

        _testErrorMessage = "Test Error Message";
        _testErrorMessageTranslatedMessage = "Test Fejlbesked";
        _testErrorStatusCode = 400;
        _testError = new Error(_testErrorMessage, _testErrorStatusCode);

        _errorFactory.CreateError(Arg.Any<string>(), Arg.Any<int>()).Returns(_testError);
        _errorTranslationService
            .GetErrorMessage(Arg.Any<Enum>(), Arg.Any<SupportedErrorLanguage>())
            .Returns(_testErrorMessageTranslatedMessage);
        
        _validator = new Validator(_errorTranslationService, _errorFactory);
    }

    // TODO - This is not valid anymore. Clearing in other situations should be tested instead!
    // [Test]
    // public void GivenValidatorHasErrors_WhenCallingToResult_ErrorsAreCleared()
    // {
    //     
    //     const string testValue = "test";
    //     
    //     _validator
    //         .ForValue(testValue)
    //         .ValidateLength( 10, 100, TestErrorCodes.InvalidLength)
    //         .GetResult<string>();
    //     
    //     Assert.That(_validator.GetErrors(), Is.Empty);
    // }

    [Test]
    public void GivenValidatorContainsNoErrors_WhenCallingToResult_ReturnOkResult()
    {
        const string testValue = "test";

        Result<string> validationResult = _validator
            .ForValue(testValue)
                .ValidateLength(0, 10, TestErrorCodes.InvalidLength)
            .ForValue(testValue)
                .ValidateLength(0, 10, TestErrorCodes.InvalidLength)
            .GetResult();
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsSuccess, Is.EqualTo(true));
            Assert.That(validationResult.Value, Is.EqualTo(testValue));
        });
    }
    
    [Test]
    public void GivenValidatorForInteger_WhenCallingValidateLength_ShowCompilerError()
    {
        var testValue = 2;

        // THIS SHOULD BE AN ERROR - DON'T WORRY!
        Result<int> validationResult = _validator
            .ForValue(testValue)
                .ValidateRange(0, 100, TestErrorCodes.InvalidRange)
            .GetResult();
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsSuccess, Is.EqualTo(true));
            Assert.That(validationResult.Value, Is.EqualTo(testValue));
        });
    }
    
    [Test]
    public void GivenValidatorContainsErrors_WhenCallingToResult_ReturnErrorResult()
    {
        const string testValue = "test";

        Result<string> validationResult = _validator
            .ForValue(testValue)
                .ValidateLength(10, 100, TestErrorCodes.InvalidLength)
            .GetResult();
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsSuccess, Is.EqualTo(false));
            Assert.That(validationResult.Errors, Has.Count.EqualTo(1));
            Assert.That(validationResult.Errors[0].Message, Is.EqualTo(_testError.Message));
            Assert.That(validationResult.Errors[0].Metadata["StatusCode"], Is.EqualTo(_testError.StatusCode));
        });
    }
}
