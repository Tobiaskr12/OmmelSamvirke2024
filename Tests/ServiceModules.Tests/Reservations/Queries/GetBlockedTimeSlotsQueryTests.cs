using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetBlockedTimeSlotsQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetBlockedTimeSlots_Valid_ReturnsList()
    {
        // Arrange
        DateTime start = DateTime.UtcNow.AddHours(1);
        var slot1 = new BlockedReservationTimeSlot { StartTime = start, EndTime = start.AddHours(1) };
        var slot2 = new BlockedReservationTimeSlot { StartTime = start.AddHours(2), EndTime = start.AddHours(3) };
        await AddTestData([slot1, slot2]);

        // Act
        var query = new GetBlockedTimeSlotsQuery(start, TimeSpan.FromHours(4));
        Result<List<BlockedReservationTimeSlot>> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Count, Is.GreaterThanOrEqualTo(2));
        });
    }

    [Test]
    public async Task GetBlockedTimeSlots_NoneFound_ReturnsEmptyList()
    {
        // Arrange
        DateTime future = DateTime.UtcNow.AddDays(5);

        // Act
        var query = new GetBlockedTimeSlotsQuery(future, TimeSpan.FromHours(2));
        Result<List<BlockedReservationTimeSlot>> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
