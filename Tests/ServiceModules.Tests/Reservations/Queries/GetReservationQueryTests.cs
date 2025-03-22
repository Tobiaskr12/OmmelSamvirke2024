using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetReservationQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetReservation_Valid_ReturnsReservation()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Test Room" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Email = "getreservation@example.com";
        reservation.Location = location;
        await AddTestData(reservation);

        // Act
        var query = new GetReservationQuery(reservation.Id);
        Result<Reservation> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Email, Is.EqualTo("getreservation@example.com"));
        });
    }

    [Test]
    public async Task GetReservation_NotFound_ReturnsFailure()
    {
        // Arrange
        var query = new GetReservationQuery(-1);

        // Act
        Result<Reservation> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.That(result.IsFailed);
    }
}
