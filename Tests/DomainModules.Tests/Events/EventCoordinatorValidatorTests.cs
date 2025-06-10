using DomainModules.Errors;
using DomainModules.Events.Entities;
using DomainModules.Events.Validators;
using FluentValidation.TestHelper;

namespace DomainModules.Tests.Events;

[TestFixture, Category("UnitTests")]
public class EventCoordinatorValidatorTests
{
    private EventCoordinatorValidator _validator;
    private EventCoordinator _validCoordinator;

    [SetUp]
    public void SetUp()
    {
        _validator = new EventCoordinatorValidator();
        _validCoordinator = new EventCoordinator
        {
            Name = "John Doe",
            EmailAddress = "john.doe@example.com",
            PhoneNumber = "+4512345678"
        };
    }

    [Test]
    public void ValidEventCoordinator_PassesValidation()
    {
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void EmptyName_FailsValidation()
    {
        _validCoordinator.Name = "";
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage(ErrorMessages.EventCoordinator_Name_NotEmpty);
    }

    [Test]
    public void NameTooShort_FailsValidation()
    {
        _validCoordinator.Name = "A";
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage(ErrorMessages.EventCoordinator_Name_InvalidLength);
    }

    [Test]
    public void InvalidEmail_FailsValidation()
    {
        _validCoordinator.EmailAddress = "invalid-email";
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldHaveValidationErrorFor(x => x.EmailAddress)
              .WithErrorMessage(ErrorMessages.EventCoordinator_EmailAddress_Invalid);
    }

    [Test]
    public void InvalidPhoneNumber_FailsValidation()
    {
        _validCoordinator.PhoneNumber = "123"; // too short and missing country code
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
              .WithErrorMessage(ErrorMessages.EventCoordinator_PhoneNumber_Invalid);
    }
    
    [Test]
    public void InvalidPhoneNumber_OnlyEmailPassesValidation()
    {
        _validCoordinator.PhoneNumber = null;
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Test]
    public void InvalidPhoneNumber_OnlyPhoneNumberPassesValidation()
    {
        _validCoordinator.EmailAddress = null;
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Test]
    public void InvalidPhoneNumber_NeitherEmailOrPhoneNumberFailsValidation()
    {
        _validCoordinator.EmailAddress = null;
        _validCoordinator.PhoneNumber = null;
        TestValidationResult<EventCoordinator>? result = _validator.TestValidate(_validCoordinator);
        Assert.That(result.Errors, Has.Count.EqualTo(1));
    }
}
