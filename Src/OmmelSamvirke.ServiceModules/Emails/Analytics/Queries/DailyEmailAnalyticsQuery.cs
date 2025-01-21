using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Analytics.Queries;

public record DailyEmailAnalyticsQuery(DateTime Date) : IRequest<Result<DailyEmailAnalytics?>>;

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
    private readonly ILogger _logger;

    public DailyEmailAnalyticsQueryHandler(IRepository<DailyEmailAnalytics> dailyEmailAnalyticsRepository, ILogger logger)
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
                "When querying for daily email analytics in {queryName}, {resultCount} analytics entities were found. Date: {date}",
                nameof(DailyEmailAnalyticsQueryHandler),
                queryResult.Value.Count,
                request.Date.ToString("dd-MM-yy")
            );
        }

        return Result.Ok(queryResult.Value.FirstOrDefault()); 
    }
}
