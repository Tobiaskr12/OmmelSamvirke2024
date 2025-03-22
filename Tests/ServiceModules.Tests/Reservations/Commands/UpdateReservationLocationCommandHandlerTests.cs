using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class UpdateReservationLocationCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task UpdateReservationLocation_Valid_ReturnsSuccess()
    {
        // Arrange
        var location = GlobalTestSetup.Fixture.Create<ReservationLocation>();
        location.Name = "Old Name";
        await AddTestData(location);
        
        // Act
        location.Name = "Updated Name";
        var command = new UpdateReservationLocationCommand(location);
        Result<ReservationLocation> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Name, Is.EqualTo("Updated Name"));
        });
    }

    [Test]
    public async Task UpdateReservationLocation_Invalid_ReturnsFailure()
    {
        // Arrange
        var location = GlobalTestSetup.Fixture.Create<ReservationLocation>();
        location.Name = "Old Name";
        await AddTestData(location);
        
        // Act
        location.Name = "X";
        var command = new UpdateReservationLocationCommand(location);
        Result<ReservationLocation> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
