using AutoFixture;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.Events.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetEventsByTimeIntervalQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetEventsByTimeInterval_ReturnsEventsWithinInterval()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        // Create several events with different start times.
        for (int i = 1; i <= 5; i++)
        {
            var ev = GlobalTestSetup.Fixture.Create<Event>();
            ev.StartTime = now.AddDays(i);
            ev.EndTime = ev.StartTime.AddHours(2);
            await AddTestData(ev);
        }
            
        DateTime startInterval = now.AddDays(2);
        DateTime endInterval = now.AddDays(4);
            
        var query = new GetEventsByTimeIntervalQuery(startInterval, endInterval);
            
        // Act
        Result<List<Event>> result = await GlobalTestSetup.Mediator.Send(query);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.GreaterThan(0));
            Assert.That(result.Value.All(e => 
                (e.StartTime >= startInterval && e.StartTime <= endInterval) ||
                (e.EndTime >= startInterval && e.EndTime <= endInterval)), Is.True);
        });
    }
    
    [Test]
    public async Task GetEventsByTimeInterval_NoEvents_ReturnsEmptyList()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        var query = new GetEventsByTimeIntervalQuery(now.AddDays(10), now.AddDays(11));
            
        // Act
        Result<List<Event>> result = await GlobalTestSetup.Mediator.Send(query);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.EqualTo(0));
        });
    }
}
