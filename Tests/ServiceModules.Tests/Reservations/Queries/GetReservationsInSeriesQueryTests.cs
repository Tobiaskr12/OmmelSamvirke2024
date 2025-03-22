using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetReservationsInSeriesQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetReservationsInSeries_Valid_ReturnsReservations()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Series Room" };
        await AddTestData(location);
        var reservation1 = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation1.Email = "series@example.com";
        reservation1.Location = location;
        reservation1.StartTime = DateTime.UtcNow.AddDays(1);
        reservation1.EndTime = DateTime.UtcNow.AddDays(1).AddHours(1);

        var reservation2 = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation2.Email = "series@example.com";
        reservation2.Location = location;
        reservation2.StartTime = DateTime.UtcNow.AddDays(2);
        reservation2.EndTime = DateTime.UtcNow.AddDays(2).AddHours(1);

        var series = new ReservationSeries
        {
            RecurrenceType = DomainModules.Reservations.Enums.RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.Date.AddDays(1),
            RecurrenceEndDate = DateTime.UtcNow.Date.AddDays(3),
            Reservations = [reservation1, reservation2]
        };
        await AddTestData(series);

        // Act
        var query = new GetReservationsInSeriesQuery(series.Id);
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Count, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetReservationsInSeries_NotFound_ReturnsFailure()
    {
        // Arrange
        var query = new GetReservationsInSeriesQuery(-1);

        // Act
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.That(result.IsFailed);
    }
}
