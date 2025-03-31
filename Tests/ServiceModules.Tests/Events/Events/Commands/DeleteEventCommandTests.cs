using AutoFixture;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.Events.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteEventCommandTests : ServiceTestBase
{
    [Test]
    public async Task DeleteEvent_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        await AddTestData(ev);
            
        var command = new DeleteEventCommand(ev.Id);
            
        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.That(result.IsSuccess);
    }
    
    [Test]
    public async Task DeleteEvent_NonExistingEvent_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteEventCommand(-1);
            
        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
