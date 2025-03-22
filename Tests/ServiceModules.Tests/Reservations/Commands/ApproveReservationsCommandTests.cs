using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using FluentResults;
using MimeKit;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class ApproveReservationsCommandTests : ServiceTestBase
{
    [Test]
    public async Task ApproveReservations_InvalidToken_ReturnsFailure()
    {
        // Arrange: Create a reservation and corresponding history
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.State = ReservationState.Pending;
        reservation.Location = new ReservationLocation { Name = "Test Location" };
        await AddTestData(reservation);

        var history = new ReservationHistory
        {
            Email = reservation.Email,
            Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act - Use an incorrect token
        var command = new ApproveReservationsCommand([reservation.Id], Guid.NewGuid());
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task ApproveReservations_Valid_ReturnsSuccessAndUpdatesState_AndSendsEmail()
    {
        // Arrange
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.State = ReservationState.Pending;
        reservation.Location = new ReservationLocation { Name = "Conference Hall" };
        await AddTestData(reservation);

        var history = new ReservationHistory
        {
            Email = reservation.Email,
            Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new ApproveReservationsCommand([reservation.Id], history.Token);
        Result<List<Reservation>> result = await GlobalTestSetup.Mediator.Send(command);

        MimeMessage? email = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "godkendt");
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.First().State, Is.EqualTo(ReservationState.Accepted));
            Assert.That(email, Is.Not.Null);
        });
    }
}
