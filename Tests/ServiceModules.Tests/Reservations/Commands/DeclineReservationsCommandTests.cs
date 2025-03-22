using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using FluentResults;
using MimeKit;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeclineReservationsCommandTests : ServiceTestBase
{
    [Test]
    public async Task DeclineReservations_Valid_ReturnsSuccess_AndSendsEmail()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Meeting Room 1" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.State = ReservationState.Pending;
        await AddTestData(reservation);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new DeclineReservationsCommand([reservation.Id], history.Token);
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        MimeMessage? email = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "afvist");
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.First().State, Is.EqualTo(ReservationState.Denied));
            Assert.That(email, Is.Not.Null);
        });
    }

    [Test]
    public async Task DeclineReservations_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Meeting Room 2" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        reservation.State = ReservationState.Pending;
        await AddTestData(reservation);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new DeclineReservationsCommand([reservation.Id], Guid.NewGuid());
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
