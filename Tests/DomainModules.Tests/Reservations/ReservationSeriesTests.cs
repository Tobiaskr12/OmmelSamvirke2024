using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using DomainModules.Reservations.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Reservations;

public class ReservationSeriesTests
{
    private ReservationSeriesValidator _validator;
    private ReservationSeries _baseValidSeries;

    [SetUp]
    public void SetUp()
    {
        _validator = new ReservationSeriesValidator();
        _baseValidSeries = new ReservationSeries
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.AddMinutes(10),
            RecurrenceEndDate = DateTime.UtcNow.AddHours(2),
            Reservations = []
        };
    }

    [Test]
    public void ValidReservationSeries_PassesValidation()
    {
        ValidationResult result = _validator.Validate(_baseValidSeries);
        Assert.That(result.IsValid);
    }

    [Test]
    public void ReservationSeries_InvalidRecurrenceType_FailsValidation()
    {
        _baseValidSeries.RecurrenceType = RecurrenceType.None;
        ValidationResult result = _validator.Validate(_baseValidSeries);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        });
    }

    [Test]
    public void ReservationSeries_InvalidInterval_FailsValidation()
    {
        _baseValidSeries.Interval = 0;
        ValidationResult resultZero = _validator.Validate(_baseValidSeries);

        _baseValidSeries.Interval = -1;
        ValidationResult resultNegative = _validator.Validate(_baseValidSeries);

        Assert.Multiple(() =>
        {
            Assert.That(resultZero.IsValid, Is.False);
            Assert.That(resultNegative.IsValid, Is.False);
        });
    }

    [Test]
    public void ReservationSeries_StartDateInPast_FailsValidation()
    {
        _baseValidSeries.RecurrenceStartDate = DateTime.UtcNow.AddMinutes(-5);
        _baseValidSeries.RecurrenceEndDate = DateTime.UtcNow.AddHours(1);
        ValidationResult result = _validator.Validate(_baseValidSeries);
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void ReservationSeries_EndDateNotAfterStart_FailsValidation()
    {
        _baseValidSeries.RecurrenceEndDate = _baseValidSeries.RecurrenceStartDate;
        ValidationResult resultEqual = _validator.Validate(_baseValidSeries);
        
        _baseValidSeries.RecurrenceEndDate = _baseValidSeries.RecurrenceStartDate.AddMinutes(-1);
        ValidationResult resultLess = _validator.Validate(_baseValidSeries);

        Assert.Multiple(() =>
        {
            Assert.That(resultEqual.IsValid, Is.False);
            Assert.That(resultLess.IsValid, Is.False);
        });
    }
}