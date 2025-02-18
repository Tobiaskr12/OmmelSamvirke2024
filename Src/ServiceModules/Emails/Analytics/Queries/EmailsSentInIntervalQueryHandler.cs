using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails.Enums;
using Contracts.ServiceModules.Emails.Analytics;
using FluentResults;
using MediatR;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.Analytics.Queries;

public class EmailsSentInIntervalQueryHandler : IRequestHandler<EmailsSentInIntervalQuery, Result<int>>
{
    private readonly IRepository<Email> _emailRepository;

    public EmailsSentInIntervalQueryHandler(IRepository<Email>  emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<Result<int>> Handle(EmailsSentInIntervalQuery request, CancellationToken cancellationToken)
    {
        DateTime startTime = request.StartTime.ToUniversalTime();
        DateTime endTime = request.Interval switch
        {
            ServiceLimitInterval.PerMinute => startTime.AddMinutes(1),
            ServiceLimitInterval.PerHour => startTime.AddHours(1),
            _ => DateTime.MinValue
        };

        if (endTime == DateTime.MinValue  || startTime >= DateTime.UtcNow)
        {
            return Result.Fail<int>(ErrorMessages.ServiceLimits_InvalidEmailInterval);
        }
        
        Result<List<Email>> queryResult = await _emailRepository.FindAsync(x => x.DateCreated > startTime && x.DateCreated <= endTime, cancellationToken: cancellationToken);
        return queryResult.IsFailed ? 
            Result.Fail<int>(ErrorMessages.GenericErrorWithRetryPrompt) : 
            Result.Ok(queryResult.Value.Count);
    }
}
