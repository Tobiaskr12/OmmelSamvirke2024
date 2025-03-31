using AutoFixture;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.EventCoordinators.Commands;

[TestFixture, Category("IntegrationTests")]
public class UpdateEventCoordinatorCommandTests : ServiceTestBase
{
    [Test]
    public async Task UpdateEventCoordinator_WithValidData_ReturnsUpdatedCoordinator()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        await AddTestData(coordinator);
            
        coordinator.PhoneNumber = "+4511122233";
        var command = new UpdateEventCoordinatorCommand(coordinator);
            
        // Act
        Result<EventCoordinator> result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.PhoneNumber, Is.EqualTo("+4511122233"));
        });
    }
    
    [Test]
    public async Task UpdateEventCoordinator_InvalidData_ReturnsFailure()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        coordinator.Name = ""; // invalid name
        var command = new UpdateEventCoordinatorCommand(coordinator);
            
        // Act
        Result<EventCoordinator> result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
