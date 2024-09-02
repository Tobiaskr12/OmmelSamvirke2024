using ErrorHandling.Helpers;
using ErrorHandling.Interfaces;
using FluentResults;
using NSubstitute;
using Error = ErrorHandling.Models.Error;

namespace ErrorHandling.Tests.Helpers;

public class ValidationHelperTests
{
    private IValidator _validator;
    
    [SetUp]
    public void Setup()
    {
        _validator = Substitute.For<IValidator>();
    }

    [Test]
    public void GivenValidatorIsSuccess_WhenCallingValidateAndReturnResult_ReturnResult()
    {
        _validator.IsSuccess().Returns(true);
        const string valueToBeValidated = "Success";

        Result<string> validationResult = ValidationHelper.GetValidationResult(valueToBeValidated, _validator);
        
        Assert.That(validationResult.IsSuccess, Is.EqualTo(true));
    }

    [Test]
    public void GivenValidatorIsFailed_WhenCallingValidateAndReturnResult_ReturnErrorResult()
    {
        List<Error> errors = [
            new Error("Test error 1", 400),
            new Error("Test error 2", 500, "StackTrace")
        ];
        _validator.IsSuccess().Returns(false);
        _validator.GetErrors().Returns(errors);
        const string valueToBeValidated = "Failure";
        
        Result<string> validationResult = ValidationHelper.GetValidationResult(valueToBeValidated, _validator);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsSuccess, Is.EqualTo(false));
            Assert.That(validationResult.Errors, Has.Count.EqualTo(errors.Count));

            for (var i = 0; i < validationResult.Errors.Count; i++)
            {
                Assert.That(validationResult.Errors[i].Message, Is.EqualTo(errors[i].Message));
                Assert.That(validationResult.Errors[i].Metadata["StatusCode"], Is.EqualTo(errors[i].StatusCode));
            }
        });
    }
}
