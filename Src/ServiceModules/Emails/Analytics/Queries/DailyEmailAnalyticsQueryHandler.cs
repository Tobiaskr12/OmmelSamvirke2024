using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.Analytics;
using Contracts.SupportModules.Logging;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.Analytics.Queries;

[UsedImplicitly]
public class DailyEmailAnalyticsQueryValidator : AbstractValidator<DateTime>
{
    public DailyEmailAnalyticsQueryValidator()
    {
        RuleFor(x => x.Date)
            .Must(x => x <= DateTime.UtcNow.Date)
            .WithMessage(ErrorMessages.DailyEmailAnalytics_DateMustBeInPast);
    }
}

public class DailyEmailAnalyticsQueryHandler : IRequestHandler<DailyEmailAnalyticsQuery, Result<DailyEmailAnalytics?>>
{
    private readonly IRepository<DailyEmailAnalytics> _dailyEmailAnalyticsRepository;
    private readonly ILoggingHandler _logger;

    public DailyEmailAnalyticsQueryHandler(IRepository<DailyEmailAnalytics> dailyEmailAnalyticsRepository, ILoggingHandler logger)
    {
        _dailyEmailAnalyticsRepository = dailyEmailAnalyticsRepository;
        _logger = logger;
    }
    
    public async Task<Result<DailyEmailAnalytics?>> Handle(DailyEmailAnalyticsQuery request, CancellationToken cancellationToken)
    {
        Result<List<DailyEmailAnalytics>> queryResult = await _dailyEmailAnalyticsRepository.FindAsync(
            x => x.Date == request.Date,
            cancellationToken: cancellationToken
        );
        
        if (queryResult.IsFailed)
        {
            return Result.Fail<DailyEmailAnalytics?>(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        
        // There should always only be one result, but we log in case there is more than one
        if (queryResult.Value.Count > 1)
        {
            _logger.LogWarning(
                $"When querying for daily email analytics in {nameof(DailyEmailAnalyticsQueryHandler)}, {queryResult.Value.Count} analytics entities were found."
            );
        }

        return Result.Ok(queryResult.Value.FirstOrDefault()); 
    }
}
