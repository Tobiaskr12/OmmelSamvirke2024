using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateReservationLocationCommandTests : ServiceTestBase
{
    [Test]
    public async Task CreateReservationLocation_Valid_ReturnsSuccess()
    {
        // Arrange
        var location = GlobalTestSetup.Fixture.Create<ReservationLocation>();

        // Act
        var command = new CreateReservationLocationCommand(location);
        Result<ReservationLocation> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task CreateReservationLocation_Invalid_ReturnsFailure()
    {
        // Arrange: invalid name
        var location = GlobalTestSetup.Fixture.Create<ReservationLocation>();
        location.Name = string.Empty;

        // Act
        var command = new CreateReservationLocationCommand(location);
        Result<ReservationLocation> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
