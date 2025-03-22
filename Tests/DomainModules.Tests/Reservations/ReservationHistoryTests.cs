using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Reservations;

[TestFixture, Category("UnitTests")]
public class ReservationHistoryTests
{
    private ReservationHistoryValidator _validator;
    private ReservationHistory _baseValidReservationHistory;
    private Reservation _baseValidReservation;

    [SetUp]
    public void SetUp()
    {
        _baseValidReservation = new Reservation
        {
            Email = "test@example.com",
            PhoneNumber = "1234567",
            Name = "Valid Reservation Name",
            StartTime = DateTime.UtcNow.AddMinutes(10),
            EndTime = DateTime.UtcNow.AddHours(2),
            CommunityName = "Valid Community",
            Location = new ReservationLocation
            {
                Name = "Beboerhuset"
            }
        };

        var reservationValidator = new ReservationValidator();
        _validator = new ReservationHistoryValidator(reservationValidator);

        _baseValidReservationHistory = new ReservationHistory
        {
            Email = "test@example.com",
            Token = Guid.NewGuid(),
            Reservations = [_baseValidReservation]
        };
    }

    [Test]
    public void ValidReservationHistory_PassesValidation()
    {
        ValidationResult result = _validator.Validate(_baseValidReservationHistory);
        Assert.That(result.IsValid);
    }

    [Test]
    public void ReservationHistory_InvalidEmail_FailsValidation()
    {
        ReservationHistory history = _baseValidReservationHistory;
        history.Email = "invalid-email";
        ValidationResult result = _validator.Validate(history);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ReservationHistory_Email_InvalidStructure)
            ));
        });
    }

    [Test]
    public void ReservationHistory_EmptyToken_FailsValidation()
    {
        ReservationHistory history = _baseValidReservationHistory;
        history.Token = Guid.Empty;
        ValidationResult result = _validator.Validate(history);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ReservationHistory_Token_Empty)
            ));
        });
    }

    [Test]
    public void ReservationHistory_InvalidReservation_FailsValidation()
    {
        var invalidReservation = new Reservation
        {
            Email = "invalid-email",
            PhoneNumber = "1234567",
            Name = "Valid Reservation Name",
            StartTime = DateTime.UtcNow.AddMinutes(10),
            EndTime = DateTime.UtcNow.AddHours(2),
            CommunityName = "Valid Community",
            Location = new ReservationLocation
            {
                Name = "Beboerhuset"
            }
        };

        ReservationHistory history = _baseValidReservationHistory;
        history.Reservations = [invalidReservation];
        ValidationResult result = _validator.Validate(history);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_Email_InvalidStructure)
            ));
        });
    }

    [Test]
    public void ReservationHistory_EmptyReservationsList_PassesValidation()
    {
        ReservationHistory history = _baseValidReservationHistory;
        history.Reservations = [];
        ValidationResult result = _validator.Validate(history);
        Assert.That(result.IsValid);
    }
}
