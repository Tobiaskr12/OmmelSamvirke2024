using Contracts.SupportModules.Logging.Models;
using Web.Components.Pages.TechnicalData.Components.Timeline.Models;

namespace Web.Components.Pages.TechnicalData.Components.Timeline;

public static class TimelineGroupingHelper
{
    public static IEnumerable<BucketedEntry> GroupByTimeInterval<T>(IEnumerable<T> items) where T : TimestampedEntry
    {
        if (!items.Any()) return [];

        var sortedItems = items.OrderBy(i => i.Timestamp).ToList();
        var firstTimestamp = sortedItems.First().Timestamp;
        var lastTimestamp = sortedItems.Last().Timestamp;
        var totalRange = lastTimestamp - firstTimestamp;

        TimeSpan interval;
        if (totalRange.TotalMinutes <= 60)
        {
            interval = TimeSpan.FromMinutes(1);
        }
        else if (totalRange.TotalHours <= 24)
        {
            interval = TimeSpan.FromHours(1);
        }
        else
        {
            interval = TimeSpan.FromDays(1);
        }

        return sortedItems
            .GroupBy(item => GetBucketStart(item.Timestamp, interval))
            .Select(group => new BucketedEntry
            {
                BucketStart = group.Key,
                Count = group.Count()
            })
            .OrderBy(x => x.BucketStart);
    }

    private static DateTime GetBucketStart(DateTime timestamp, TimeSpan interval)
    {
        if (interval.TotalDays >= 1)
        {
            return timestamp.Date;
        }
        else if (interval.TotalHours >= 1)
        {
            return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
        }
        else
        {
            return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0);
        }
    }
}
