using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteReservationCommandTests : ServiceTestBase
{
    [Test]
    public async Task DeleteReservation_Valid_ReturnsSuccess()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Office" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        await AddTestData(reservation);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);


        // Act
        var command = new DeleteReservationCommand(reservation.Id, history.Token);
        Result result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task DeleteReservation_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var location = new ReservationLocation { Name = "Office 2" };
        await AddTestData(location);
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Location = location;
        await AddTestData(reservation);

        var history = new ReservationHistory
        {
            Email = reservation.Email, Reservations = [reservation],
            Token = Guid.NewGuid()
        };
        await AddTestData(history);
        
        // Act
        var command = new DeleteReservationCommand(reservation.Id, Guid.NewGuid());
        Result result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
