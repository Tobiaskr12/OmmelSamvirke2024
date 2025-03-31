using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Common;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteRecurringReservationsCommandTests : ServiceTestBase
{
    [Test]
    public async Task DeleteRecurringReservations_Valid_ReturnsSuccess()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Event Hall" };
        await AddTestData(location);
        var reservation1 = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation1.Location = location;
        reservation1.StartTime = DateTime.UtcNow.AddDays(2);
        reservation1.EndTime = DateTime.UtcNow.AddDays(2).AddHours(1);

        var reservation2 = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation2.Email = reservation1.Email;
        reservation2.Location = location;
        reservation2.StartTime = DateTime.UtcNow.AddDays(4);
        reservation2.EndTime = DateTime.UtcNow.AddDays(4).AddHours(1);

        var series = new ReservationSeries
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.Date.AddDays(2),
            RecurrenceEndDate = DateTime.UtcNow.Date.AddDays(4),
            Reservations = [reservation1, reservation2]
        };
        await AddTestData(series);

        var history = new ReservationHistory
        {
            Email = reservation1.Email, Reservations = [reservation1, reservation2],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new DeleteRecurringReservationsCommand(series.Id, DateTime.UtcNow.AddDays(3), history.Token);
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert – only reservations from day 3 onward should be deleted
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value.First().StartTime, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddDays(3)));
        });
    }

    [Test]
    public async Task DeleteRecurringReservations_NoReservationsToDelete_ReturnsEmptyList()
    {
        // Arrange: series with reservations before cutoff
        var location = new ReservationLocation { Name = "Small Room" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.StartTime = DateTime.UtcNow.AddDays(-1);
        reservation.EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1);

        var series = new ReservationSeries
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.Date.AddDays(-2),
            RecurrenceEndDate = DateTime.UtcNow.Date.AddDays(-1),
            Reservations = [reservation]
        };
        await AddTestData(series);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new DeleteRecurringReservationsCommand(series.Id, DateTime.UtcNow, history.Token);
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task DeleteRecurringReservations_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Auditorium" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.StartTime = DateTime.UtcNow.AddDays(2);
        reservation.EndTime = DateTime.UtcNow.AddDays(2).AddHours(1);

        var series = new ReservationSeries
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.Date.AddDays(2),
            RecurrenceEndDate = DateTime.UtcNow.Date.AddDays(3),
            Reservations = [reservation]
        };
        await AddTestData(series);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new DeleteRecurringReservationsCommand(series.Id, DateTime.UtcNow.AddDays(2), Guid.NewGuid());
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
