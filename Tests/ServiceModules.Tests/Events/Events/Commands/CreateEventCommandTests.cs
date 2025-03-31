using AutoFixture;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Common;
using DomainModules.Events.Entities;
using DomainModules.Reservations.Entities;
using FluentResults;

namespace ServiceModules.Tests.Events.Events.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateEventCommandTests : ServiceTestBase
{
    [Test]
    public async Task CreateSingleEvent_ReturnsSuccess()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        await AddTestData(coordinator);
        
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        ev.EventCoordinator = coordinator;
        ev.StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        ev.EndTime = ev.StartTime.AddHours(2);
        ev.Location = "Main Hall";
        
        var command = new CreateEventCommand(ev, null, null);
        
        // Act
        Result<List<Event>> result = await GlobalTestSetup.Mediator.Send(command);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Empty);
            Assert.That(result.Value, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task CreateRecurringEventWithReservation_ReturnsMultipleEventsAndReservations()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        await AddTestData(coordinator);
        
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        ev.EventCoordinator = coordinator;
        ev.StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        ev.EndTime = ev.StartTime.AddHours(1);
        ev.Location = "Conference Room A";
        
        var recurrenceOptions = new RecurrenceOptions
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.AddDays(1).Date,
            RecurrenceEndDate = DateTime.UtcNow.AddDays(3).Date
        };
        
        // Create a base reservation whose time portion will be applied to each event occurrence
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.StartTime = ev.StartTime;
        reservation.EndTime = ev.EndTime;
        
        var command = new CreateEventCommand(ev, recurrenceOptions, reservation);
        
        // Act
        Result<List<Event>> result = await GlobalTestSetup.Mediator.Send(command);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.EqualTo(3));
            foreach (Event createdEvent in result.Value)
            {
                Assert.That(createdEvent.Reservation, Is.Not.Null);
                Assert.That(createdEvent.StartTime.Date, Is.EqualTo(createdEvent.Reservation?.StartTime.Date));
            }
        });
    }
    
    [Test]
    public async Task CreateRecurringEventWithReservation_InvalidReservationData_ReturnsFailure()
    {
        // Arrange
        var coordinator = GlobalTestSetup.Fixture.Create<EventCoordinator>();
        await AddTestData(coordinator);
        
        var ev = GlobalTestSetup.Fixture.Create<Event>();
        ev.EventCoordinator = coordinator;
        ev.StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8);
        ev.EndTime = ev.StartTime.AddHours(1);
        ev.Location = "Conference Room B";
        
        var recurrenceOptions = new RecurrenceOptions
        {
            RecurrenceType = RecurrenceType.Daily,
            Interval = 1,
            RecurrenceStartDate = DateTime.UtcNow.AddDays(1).Date,
            RecurrenceEndDate = DateTime.UtcNow.AddDays(3).Date
        };
        
        // Create an invalid reservation (e.g. empty email)
        var reservation = GlobalTestSetup.Fixture.Create<Reservation>();
        reservation.Email = "";
        reservation.StartTime = ev.StartTime;
        reservation.EndTime = ev.EndTime;
        
        var command = new CreateEventCommand(ev, recurrenceOptions, reservation);
        
        // Act
        Result<List<Event>> result = await GlobalTestSetup.Mediator.Send(command);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
