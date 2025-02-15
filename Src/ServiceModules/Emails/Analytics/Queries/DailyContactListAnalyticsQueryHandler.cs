using Contracts.DataAccess.Base;
using Contracts.Emails.Analytics.Queries;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.Analytics.Queries;

[UsedImplicitly]
public class DailyContactListAnalyticsQueryValidator : AbstractValidator<DateTime>
{
    public DailyContactListAnalyticsQueryValidator()
    {
        RuleFor(x => x)
            .Must(date => date <= DateTime.UtcNow.Date)
            .WithMessage(ErrorMessages.DailyEmailAnalytics_DateMustBeInPast);
    }
}

public class DailyContactListAnalyticsQueryHandler : IRequestHandler<DailyContactListAnalyticsQuery, Result<List<DailyContactListAnalytics>>>
{
    private readonly IRepository<DailyContactListAnalytics> _dailyContactListAnalyticsRepository;

    public DailyContactListAnalyticsQueryHandler(IRepository<DailyContactListAnalytics> dailyContactListAnalyticsRepository)
    {
        _dailyContactListAnalyticsRepository = dailyContactListAnalyticsRepository;
    }

    public async Task<Result<List<DailyContactListAnalytics>>> Handle(DailyContactListAnalyticsQuery request, CancellationToken cancellationToken)
    {
        Result<List<DailyContactListAnalytics>> queryResult = await _dailyContactListAnalyticsRepository.FindAsync(
            x => x.Date == request.Date,
            cancellationToken: cancellationToken);

        return queryResult.IsFailed ? 
            Result.Fail<List<DailyContactListAnalytics>>(ErrorMessages.GenericErrorWithRetryPrompt) : 
            Result.Ok(queryResult.Value);
    }
}
