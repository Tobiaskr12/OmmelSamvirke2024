using AutoFixture;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.EventCoordinators.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetAllEventCoordinatorsQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetAllEventCoordinators_ReturnsAllCoordinators()
    {
        // Arrange
        var coordinator1 = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        coordinator1.Name = "Coordinator One";
        var coordinator2 = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        coordinator2.Name = "Coordinator Two";
        await AddTestData(coordinator1);
        await AddTestData(coordinator2);
            
        var query = new GetAllEventCoordinatorsQuery();
            
        // Act
        Result<List<EventCoordinator>> result = await GlobalTestSetup.Mediator.Send(query);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.EqualTo(2));
        });
    }
    
    [Test]
    public async Task GetAllEventCoordinators_NoCoordinators_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllEventCoordinatorsQuery();
            
        // Act
        Result<List<EventCoordinator>> result = await GlobalTestSetup.Mediator.Send(query);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.EqualTo(0));
        });
    }
}
