using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetReservationsQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetReservations_Valid_ReturnsList()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Common Area" };
        await AddTestData(location);
        var reservation1 = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation1.Email = "getlist@example.com";
        reservation1.Location = location;
        reservation1.StartTime = DateTime.UtcNow.AddHours(1);
        reservation1.EndTime = DateTime.UtcNow.AddHours(2);
        var reservation2 = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation2.Email = "getlist@example.com";
        reservation2.Location = location;
        reservation2.StartTime = DateTime.UtcNow.AddHours(3);
        reservation2.EndTime = DateTime.UtcNow.AddHours(4);
        await AddTestData([reservation1, reservation2]);
        
        // Act
        var query = new GetReservationsQuery(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromHours(5));
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.GreaterThanOrEqualTo(2));
        });
    }

    [Test]
    public async Task GetReservations_NoneFound_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetReservationsQuery(DateTime.UtcNow.AddDays(10), TimeSpan.FromHours(2));

        // Act
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
