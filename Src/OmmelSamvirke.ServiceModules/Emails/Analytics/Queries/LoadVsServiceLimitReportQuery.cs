using FluentResults;
using MediatR;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Analytics.Queries;

public record LoadVsServiceLimitReportQuery(DateTime StartTime, ServiceLimitInterval Interval) : IRequest<Result<LoadVsServiceLimitReport>>;

public record LoadVsServiceLimitReport(List<int> SegmentedCounts, List<int> CumulativeCounts);

public class LoadVsServiceLimitReportQueryHandler : IRequestHandler<LoadVsServiceLimitReportQuery, Result<LoadVsServiceLimitReport>>
{
    private readonly IRepository<Email> _emailRepository;

    public LoadVsServiceLimitReportQueryHandler(IRepository<Email> emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<Result<LoadVsServiceLimitReport>> Handle(LoadVsServiceLimitReportQuery request, CancellationToken cancellationToken)
    {
        DateTime startTime = request.StartTime.ToUniversalTime();
        if (startTime >= DateTime.UtcNow)
        {
            return Result.Fail<LoadVsServiceLimitReport>(ErrorMessages.ServiceLimits_InvalidEmailInterval);
        }
        
        TimeSpan subIntervalDuration;
        int dataPointsCount;
        switch (request.Interval)
        {
            case ServiceLimitInterval.PerMinute:
                // 12 sub-intervals of 5 seconds each
                subIntervalDuration = TimeSpan.FromSeconds(5);
                dataPointsCount = 12;
                break;
            case ServiceLimitInterval.PerHour:
                // 12 sub-intervals of 5 minutes each
                subIntervalDuration = TimeSpan.FromMinutes(5);
                dataPointsCount = 12;
                break;
            default:
                return Result.Fail<LoadVsServiceLimitReport>(ErrorMessages.ServiceLimits_InvalidEmailInterval);
        }
        
        DateTime overallEndTime = startTime.AddTicks(subIntervalDuration.Ticks * dataPointsCount);
        Result<List<Email>> emailsResult = await _emailRepository.FindAsync(
            x => x.DateCreated >= startTime && x.DateCreated <= overallEndTime,
            cancellationToken: cancellationToken
        );

        if (emailsResult.IsFailed)
        {
            return Result.Fail<LoadVsServiceLimitReport>(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        
        List<Email> emails = emailsResult.Value;
        List<int> cumulativeDataset = CreateCumulativeDataset(dataPointsCount, startTime, subIntervalDuration, emails);
        List<int> segmentedDataset = CreateSegmentedDataset(dataPointsCount, cumulativeDataset);

        var report = new LoadVsServiceLimitReport(segmentedDataset, cumulativeDataset);
        return Result.Ok(report);
    }

    private static List<int> CreateSegmentedDataset(int dataPointsCount, List<int> cumulativeDataset)
    {
        var segmentedCounts = new List<int>(dataPointsCount);
        for (int i = 0; i < dataPointsCount; i++)
        {
            if (i == 0)
            {
                segmentedCounts.Add(0);
            }
            else
            {
                segmentedCounts.Add(cumulativeDataset[i] - cumulativeDataset[i - 1]);
            }
        }

        return segmentedCounts;
    }

    private static List<int> CreateCumulativeDataset(int dataPointsCount, DateTime startTime, TimeSpan subIntervalDuration,
        List<Email> emails)
    {
        var cumulativeCounts = new List<int>(dataPointsCount);
        for (int i = 0; i < dataPointsCount; i++)
        {
            DateTime boundary = startTime.AddTicks(subIntervalDuration.Ticks * (i + 1));
            int cumulative = emails.Count(e => e.DateCreated <= boundary);
            cumulativeCounts.Add(cumulative);
        }

        return cumulativeCounts;
    }
}
