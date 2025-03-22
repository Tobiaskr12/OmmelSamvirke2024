using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Reservations;

[TestFixture, Category("UnitTests")]
public class ReservationTests
{
    private ReservationValidator _validator;
    private Reservation _baseValidReservation;

    [SetUp]
    public void SetUp()
    {
        _validator = new ReservationValidator();
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
    }

    [Test]
    public void ValidReservation_PassesValidation()
    {
        ValidationResult result = _validator.Validate(_baseValidReservation);
        Assert.That(result.IsValid);
    }

    [Test]
    public void Reservation_InvalidEmail_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.Email = "invalid-email";
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_Email_InvalidStructure)
            ));
        });
    }

    [Test]
    public void Reservation_EmptyPhoneNumber_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.PhoneNumber = "";
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_PhoneNumber_Empty)
            ));
        });
    }

    [Test]
    public void Reservation_PhoneNumberTooShort_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.PhoneNumber = "1234";
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_PhoneNumber_InvalidLength)
            ));
        });
    }

    [Test]
    public void Reservation_PhoneNumberTooLong_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.PhoneNumber = new string('1', 25);
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_PhoneNumber_InvalidLength)
            ));
        });
    }

    [Test]
    public void Reservation_EmptyName_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.Name = "";
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_Name_Empty)
            ));
        });
    }

    [Test]
    public void Reservation_NameTooShort_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.Name = "ab";
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_Name_InvalidLength)
            ));
        });
    }

    [Test]
    public void Reservation_NameTooLong_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.Name = new string('a', 101);
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_Name_InvalidLength)
            ));
        });
    }

    [Test]
    public void Reservation_NullCommunityName_PassesValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.CommunityName = null;
        ValidationResult result = _validator.Validate(reservation);
        Assert.That(result.IsValid);
    }

    [Test]
    public void Reservation_CommunityNameTooShort_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.CommunityName = "ab";
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_CommunityName_InvalidLength)
            ));
        });
    }

    [Test]
    public void Reservation_CommunityNameTooLong_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.CommunityName = new string('c', 76);
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_CommunityName_InvalidLength)
            ));
        });
    }

    [Test]
    public void Reservation_StartTimeInPast_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.StartTime = DateTime.UtcNow.AddMinutes(-5);
        reservation.EndTime = reservation.StartTime.AddHours(2);
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_StartTIme_InPast)
            ));
        });
    }

    [Test]
    public void Reservation_EndTimeTooEarly_FailsValidation()
    {
        Reservation reservation = _baseValidReservation;
        reservation.EndTime = reservation.StartTime.AddMinutes(30);
        ValidationResult result = _validator.Validate(reservation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Reservation_EndTime_MustBeAfterStart)
            ));
        });
    }
}
