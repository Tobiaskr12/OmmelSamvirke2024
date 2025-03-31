using AutoFixture;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.EventCoordinators.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteEventCoordinatorCommandTests : ServiceTestBase
{
    [Test]
    public async Task DeleteEventCoordinator_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        await AddTestData(coordinator);
            
        var command = new DeleteEventCoordinatorCommand(coordinator.Id);
            
        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.That(result.IsSuccess);
    }
    
    [Test]
    public async Task DeleteEventCoordinator_NonExisting_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteEventCoordinatorCommand(-1);
            
        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
