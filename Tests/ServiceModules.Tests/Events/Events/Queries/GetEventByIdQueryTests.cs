using AutoFixture;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.Events.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetEventByIdQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetEventById_WithValidId_ReturnsEvent()
    {
        // Arrange
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        await AddTestData(ev);
            
        var query = new GetEventByIdQuery(ev.Id);
            
        // Act
        Result<Event> result = await GlobalTestSetup.Mediator.Send(query);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Null);
        });
    }
    
    [Test]
    public async Task GetEventById_NonExistingId_ReturnsFailure()
    {
        // Arrange
        var query = new GetEventByIdQuery(-1);
            
        // Act
        Result<Event> result = await GlobalTestSetup.Mediator.Send(query);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
