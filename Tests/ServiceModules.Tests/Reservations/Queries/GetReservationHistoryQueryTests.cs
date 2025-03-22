using AutoFixture;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetReservationHistoryQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetReservationHistory_Valid_ReturnsHistory()
    {
        // Arrange
        var history = GlobalTestSetup.Fixture.Create<ReservationHistory>();
        history.Email = "history@example.com";
        history.Token = Guid.NewGuid();
        await AddTestData(history);
        
        // Act
        var query = new GetReservationsHistoryQuery(history.Token);
        Result<ReservationHistory> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Email, Is.EqualTo("history@example.com"));
        });
    }

    [Test]
    public async Task GetReservationHistory_NotFound_ReturnsFailure()
    {
        // Arrange
        var query = new GetReservationsHistoryQuery(Guid.NewGuid());

        // Act
        Result<ReservationHistory> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.That(result.IsFailed);
    }
}
