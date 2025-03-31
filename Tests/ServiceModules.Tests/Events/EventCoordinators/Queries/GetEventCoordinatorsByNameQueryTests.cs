using AutoFixture;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.EventCoordinators.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetEventCoordinatorsByNameQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetEventCoordinatorsByName_ReturnsMatchingCoordinators()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        coordinator.Name = "UniqueCoordinatorName";
        coordinator.EmailAddress = "unique@coordinator.com";
        coordinator.PhoneNumber = "+4512345678";
        await AddTestData(coordinator);
            
        var query = new GetEventCoordinatorsByNameQuery("Unique");
            
        // Act
        Result<List<EventCoordinator>> result = await GlobalTestSetup.Mediator.Send(query);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Any(ec => ec.Name.Contains("Unique")));
        });
    }
    
    [Test]
    public async Task GetEventCoordinatorsByName_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetEventCoordinatorsByNameQuery("NonExistent");
            
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
