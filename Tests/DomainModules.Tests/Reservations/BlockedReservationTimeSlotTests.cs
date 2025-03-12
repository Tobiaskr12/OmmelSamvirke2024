using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Reservations;

[TestFixture, Category("UnitTests")]
public class BlockedReservationTimeSlotTests
{
    private BlockedReservationTimeSlotValidator _validator;
    private BlockedReservationTimeSlot _baseValidTimeSlot;

    [SetUp]
    public void SetUp()
    {
        _validator = new BlockedReservationTimeSlotValidator();
        _baseValidTimeSlot = new BlockedReservationTimeSlot
        {
            StartTime = DateTime.UtcNow.AddMinutes(5),
            EndTime = DateTime.UtcNow.AddHours(2)
        };
    }

    [Test]
    public void ValidBlockedReservationTimeSlot_PassesValidation()
    {
        ValidationResult result = _validator.Validate(_baseValidTimeSlot);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void BlockedReservationTimeSlot_InvalidEndTime_FailsValidation()
    {
        BlockedReservationTimeSlot timeSlot = _baseValidTimeSlot;
        timeSlot.EndTime = timeSlot.StartTime.AddMinutes(30);
        ValidationResult result = _validator.Validate(timeSlot);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.BlockedReservationTimeSlot_EndTime_MustBeAfterStart)
            ));
        });
    }

    [Test]
    public void BlockedReservationTimeSlot_WithStartTimeInPast_FailsValidation()
    {
        var timeSlot = new BlockedReservationTimeSlot
        {
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow.AddHours(1)
        };
        ValidationResult result = _validator.Validate(timeSlot);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.BlockedReservationTimeSlot_StartTime_InPast)
            ));
        });
    }

    [Test]
    public void BlockedReservationTimeSlot_WithBothInvalid_FailsValidation()
    {
        var timeSlot = new BlockedReservationTimeSlot
        {
            StartTime = DateTime.UtcNow.AddMinutes(-20),
            EndTime = DateTime.UtcNow.AddMinutes(-10)
        };
        ValidationResult result = _validator.Validate(timeSlot);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.BlockedReservationTimeSlot_StartTime_InPast)
            ));
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.BlockedReservationTimeSlot_EndTime_MustBeAfterStart)
            ));
        });
    }
}
