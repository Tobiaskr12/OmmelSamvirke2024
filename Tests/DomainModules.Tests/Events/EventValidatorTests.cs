using DomainModules.BlobStorage.Validators;
using DomainModules.Errors;
using DomainModules.Events.Entities;
using DomainModules.Events.Validators;
using FluentValidation.TestHelper;

namespace DomainModules.Tests.Events;

[TestFixture, Category("UnitTests")]
public class EventValidatorTests
{
    private EventValidator _validator;
    private Event _validEvent;
    private EventCoordinator _validCoordinator;

    [SetUp]
    public void SetUp()
    {
        var coordinatorValidator = new EventCoordinatorValidator();
        var remoteFileValidator = new BlobStorageFileValidator();
        _validator = new EventValidator(coordinatorValidator, remoteFileValidator);

        _validCoordinator = new EventCoordinator
        {
            Name = "Jane Smith",
            EmailAddress = "jane.smith@example.com",
            PhoneNumber = "12345678"
        };
        
        _validEvent = new Event
        {
            Title = "Sample Event",
            Description = "This is a valid event description.",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            EventCoordinator = _validCoordinator,
            Location = "Conference Room"
        };
    }

    [Test]
    public void ValidEvent_PassesValidation()
    {
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void EmptyTitle_FailsValidation()
    {
        _validEvent.Title = "";
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage(ErrorMessages.Event_Title_NotEmpty);
    }

    [Test]
    public void TitleTooShort_FailsValidation()
    {
        _validEvent.Title = "AB";
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage(ErrorMessages.Event_Title_InvalidLength);
    }

    [Test]
    public void DescriptionTooLong_FailsValidation()
    {
        _validEvent.Description = new string('a', 5001);
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage(ErrorMessages.Event_Description_InvalidLength);
    }

    [Test]
    public void StartTimeInPast_FailsValidation()
    {
        _validEvent.StartTime = DateTime.UtcNow.AddMinutes(-10);
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
              .WithErrorMessage(ErrorMessages.Event_StartTime_MustBeInFuture);
    }

    [Test]
    public void EndTimeBeforeStartTime_FailsValidation()
    {
        _validEvent.EndTime = _validEvent.StartTime.AddMinutes(-5);
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
              .WithErrorMessage(ErrorMessages.Event_EndTime_MustBeAfterStart);
    }

    [Test]
    public void DurationLessThan15Minutes_FailsValidation()
    {
        _validEvent.EndTime = _validEvent.StartTime.AddMinutes(10);
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
              .WithErrorMessage(ErrorMessages.Event_Duration_Minimum15Minutes);
    }

    [Test]
    public void NullEventCoordinator_FailsValidation()
    {
        _validEvent.EventCoordinator = null!;
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.EventCoordinator)
              .WithErrorMessage(ErrorMessages.Event_EventCoordinator_NotNull);
    }

    [Test]
    public void InvalidLocation_FailsValidation()
    {
        _validEvent.Location = "AB";
        TestValidationResult<Event>? result = _validator.TestValidate(_validEvent);
        result.ShouldHaveValidationErrorFor(x => x.Location)
              .WithErrorMessage(ErrorMessages.Event_Location_InvalidLength);
    }
}
