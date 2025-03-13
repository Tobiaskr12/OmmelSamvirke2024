using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Validators;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace DomainModules.Tests.Reservations;

[TestFixture, Category("UnitTests")]
public class ReservationLocationTests
{

    private ReservationLocation _baseValidReservationLocation;
    private ReservationLocationValidator _validator;
    
    [SetUp]
    public void SetUp()
    {
        _baseValidReservationLocation = new ReservationLocation
        {
            Name = "TestName"
        };

        _validator = new ReservationLocationValidator();
    }
    
    [Test]
    public void ValidReservationHistory_PassesValidation()
    {
        ValidationResult result = _validator.Validate(_baseValidReservationLocation);
        Assert.That(result.IsValid);
    }
    
    [TestCase(0)]
    [TestCase(2)]
    [TestCase(76)]
    public void ReservationHistory_InvalidNameLength_FailsValidation(int nameLength)
    {
        ReservationLocation location = _baseValidReservationLocation;
        location.Name = new string('a', nameLength);
        ValidationResult result = _validator.Validate(location);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ReservationLocation_Name_InvalidLength)
            ));
        });
    }
}
