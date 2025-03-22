using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Enums;

namespace ServiceModules.Reservations;

public static class RecurringDatesHelper
{
    public static IEnumerable<DateTime> GenerateRecurrenceDates(RecurrenceOptions options)
    {
        var dates = new List<DateTime>();

        if (options is { RecurrenceType: RecurrenceType.Custom, CustomDates: not null })
        {
            dates.AddRange(options.CustomDates);
        }
        else
        {
            DateTime current = options.RecurrenceStartDate.Date;
            DateTime end = options.RecurrenceEndDate.Date;
            
            while (current <= end)
            {
                dates.Add(current);
                current = options.RecurrenceType switch
                {
                    RecurrenceType.Daily => current.AddDays(options.Interval),
                    RecurrenceType.Weekly => current.AddDays(7 * options.Interval),
                    RecurrenceType.Monthly => current.AddMonths(options.Interval),
                    _ => end.AddDays(1)
                };
            }
        }

        return dates;
    }
}
