using AutoFixture;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.EventCoordinators.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateEventCoordinatorCommandTests : ServiceTestBase
{
    [Test]
    public async Task CreateEventCoordinator_ReturnsCoordinator()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        coordinator.Name = "Test Coordinator";
        coordinator.EmailAddress = "test@coordinator.com";
        coordinator.PhoneNumber = "+4512345678";
        var command = new CreateEventCoordinatorCommand(coordinator);
            
        // Act
        Result<EventCoordinator> result = await GlobalTestSetup.Mediator.Send(command);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Name, Is.EqualTo("Test Coordinator"));
        });
    }
    
    [Test]
    public async Task CreateEventCoordinator_InvalidData_ReturnsFailure()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        coordinator.Name = ""; // invalid empty name
        var command = new CreateEventCoordinatorCommand(coordinator);
            
        // Act
        Result<EventCoordinator> result = await GlobalTestSetup.Mediator.Send(command);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
