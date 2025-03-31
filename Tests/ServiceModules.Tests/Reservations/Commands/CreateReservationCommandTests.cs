using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Common;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using FluentResults;
using MimeKit;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateReservationCommandTests : ServiceTestBase
{
    [Test]
    public async Task CreateReservation_SingleReservation_ReturnsSuccess_AndSendsEmail()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Conference Room A" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.StartTime = DateTime.UtcNow.AddHours(2);
        reservation.EndTime = DateTime.UtcNow.AddHours(3);
        var command = new CreateReservationCommand(reservation, null);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = new List<Reservation>(),
            Token = Guid.NewGuid()
        };
        await AddTestData(history);

        // Act
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        MimeMessage? email = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "Bekræft");
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Empty);
            Assert.That(email, Is.Not.Null);
        });
    }

    [Test]
    public async Task CreateReservation_WithRecurrence_ReturnsSuccess_AndSendsEmail()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Conference Room B" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.StartTime = DateTime.UtcNow.AddHours(2);
        reservation.EndTime = DateTime.UtcNow.AddHours(3);
        var recurrenceOptions = new RecurrenceOptions
        {
            RecurrenceType = RecurrenceType.Weekly,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.Date.AddDays(1),
            RecurrenceEndDate = DateTime.UtcNow.Date.AddDays(15)
        };
        var command = new CreateReservationCommand(reservation, recurrenceOptions);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = new List<Reservation>(),
            Token = Guid.NewGuid()
        };
        await AddTestData(history);

        // Act
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        MimeMessage? email = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "Bekræft");
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Count, Is.GreaterThan(1));
            Assert.That(result.Value.All(r => r.ReservationSeriesId != null), Is.True);
            Assert.That(email, Is.Not.Null);
        });
    }

    [Test]
    public async Task CreateReservation_Conflict_ReturnsFailure()
    {
        // Arrange: create an existing reservation
        var location = new ReservationLocation { Name = "Conference Room C" };
        await AddTestData(location);
        var existingReservation = GlobalTestSetup.Fixture.Create<Reservation>();
        existingReservation.Location = location;
        existingReservation.State = ReservationState.Accepted;
        existingReservation.StartTime = DateTime.UtcNow.AddHours(2);
        existingReservation.EndTime = DateTime.UtcNow.AddHours(3);
        await AddTestData(existingReservation);

        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.StartTime = existingReservation.StartTime.AddMinutes(30);
        reservation.EndTime = existingReservation.EndTime.AddMinutes(30);
        var command = new CreateReservationCommand(reservation, null);

        // Act
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task CreateReservation_InvalidRecurrencePeriod_ReturnsFailure()
    {
        // Arrange: invalid recurrence (end before start)
        var location = new ReservationLocation { Name = "Conference Room D" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.StartTime = DateTime.UtcNow.AddHours(2);
        reservation.EndTime = DateTime.UtcNow.AddHours(3);
        var recurrenceOptions = new RecurrenceOptions
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.Date.AddDays(5),
            RecurrenceEndDate = DateTime.UtcNow.Date.AddDays(2)
        };
        var command = new CreateReservationCommand(reservation, recurrenceOptions);

        // Act
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
