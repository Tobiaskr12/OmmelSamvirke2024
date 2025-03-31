using AutoFixture;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.Events.Commands;

[TestFixture, Category("IntegrationTests")]
public class UpdateEventCommandTests : ServiceTestBase
{
    [Test]
    public async Task UpdateEvent_WithValidData_ReturnsUpdatedEvent()
    {
        // Arrange
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        ev.Location = "Old Location";
        await AddTestData(ev);
        
        // Modify properties
        ev.Location = "New Updated Location";
        ev.Title = "Updated Title";
            
        var command = new UpdateEventCommand(ev);
            
        // Act
        Result<Event> result = await GlobalTestSetup.Mediator.Send(command);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Location, Is.EqualTo("New Updated Location"));
            Assert.That(result.Value.Title, Is.EqualTo("Updated Title"));
        });
    }
    
    [Test]
    public async Task UpdateEvent_NonExistingEvent_ReturnsFailure()
    {
        // Arrange
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        ev.Id = -1; // non-existent ID
        ev.Location = "Nowhere";
        var command = new UpdateEventCommand(ev);
        
        // Act
        Result<Event> result = await GlobalTestSetup.Mediator.Send(command);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
