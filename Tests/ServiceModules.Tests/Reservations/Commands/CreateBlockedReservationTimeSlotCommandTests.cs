using Contracts.ServiceModules.Reservations;
using DomainModules.Common;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Reservations.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateBlockedReservationTimeSlotCommandTests : ServiceTestBase
{
    [Test]
    public async Task CreateBlockedReservationTimeSlot_NoRecurrence_ReturnsSuccess()
    {
        // Arrange
        DateTime start = DateTime.UtcNow.AddHours(1);
        var blockedSlot = new BlockedReservationTimeSlot { StartTime = start, EndTime = start.AddHours(1) };

        // Act
        var command = new CreateBlockedReservationTimeSlotCommand(blockedSlot, null);
        Result<List<BlockedReservationTimeSlot>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value[0].StartTime, Is.EqualTo(start).Within(TimeSpan.FromSeconds(1)));
        });
    }

    [Test]
    public async Task CreateBlockedReservationTimeSlot_WithRecurrence_ReturnsSuccess()
    {
        // Arrange
        DateTime start = DateTime.UtcNow.AddHours(1);
        var blockedSlot = new BlockedReservationTimeSlot { StartTime = start, EndTime = start.AddHours(1) };
        var recurrenceOptions = new RecurrenceOptions
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = start.Date,
            RecurrenceEndDate = start.Date.AddDays(2)
        };

        // Act
        var command = new CreateBlockedReservationTimeSlotCommand(blockedSlot, recurrenceOptions);
        Result<List<BlockedReservationTimeSlot>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert – returns first created slot
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value[0].StartTime, Is.EqualTo(start).Within(TimeSpan.FromSeconds(1)));
        });
    }

    [Test]
    public async Task CreateBlockedReservationTimeSlot_InvalidTime_ReturnsFailure()
    {
        // Arrange: start time in the past
        DateTime past = DateTime.UtcNow.AddHours(-1);
        var blockedSlot = new BlockedReservationTimeSlot { StartTime = past, EndTime = past.AddHours(1) };

        // Act
        var command = new CreateBlockedReservationTimeSlotCommand(blockedSlot, null);
        Result<List<BlockedReservationTimeSlot>> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsFailed);
    }
}
