using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using DomainModules.Events.Entities;
using FluentResults;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.Extensions.Configuration;

namespace ServiceModules.Events.IcsFeed;

public class IcsFeedService
{
    private readonly IRepository<Event> _eventRepository;
    private readonly ILoggingHandler _logger;
    private readonly string _icsFilePath;

    public IcsFeedService(
        IRepository<Event> eventRepository,
        ILoggingHandler logger,
        IConfiguration configuration)
    {
        _eventRepository = eventRepository;
        _logger = logger;
        _icsFilePath = configuration["CalendarFilePath"] ?? string.Empty;
    }

    public async Task UpdateCalendarFile(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_icsFilePath)) return;
            
            Result<List<Event>> eventsResult = await _eventRepository.GetAllAsync(cancellationToken: cancellationToken);
            if (eventsResult.IsFailed || eventsResult.Value is null)
            {
                _logger.LogError(new Exception("Unable to retrieve events for ICS update."));
                return;
            }
        
            var calendar = new Calendar();

            foreach (Event ev in eventsResult.Value)
            {
                var calendarEvent = new CalendarEvent
                {
                    Summary = ev.Title,
                    Description = ev.Description,
                    Start = new CalDateTime(ev.StartTime, "UTC"),
                    End = new CalDateTime(ev.EndTime, "UTC"),
                    Location = ev.Location,
                    Uid = ev.Uid.ToString()
                };

                calendar.Events.Add(calendarEvent);
            }

            // Serialize the calendar to an ICS-formatted string
            var serializer = new CalendarSerializer(new SerializationContext());
            string calendarString = serializer.SerializeToString(calendar);

            // Write the ICS data to file
            await File.WriteAllTextAsync(_icsFilePath, calendarString, cancellationToken);
            _logger.LogInformation($"ICS feed updated successfully at {_icsFilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
        }
    }
}
