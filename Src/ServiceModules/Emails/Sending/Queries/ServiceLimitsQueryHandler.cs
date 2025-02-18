using Contracts.DataAccess.Emails.Enums;
using Contracts.ServiceModules.Emails.Sending;
using FluentResults;
using MediatR;
using DomainModules.Emails.Constants;
using ServiceModules.Errors;

namespace ServiceModules.Emails.Sending.Queries;

public class ServiceLimitsQueryHandler : IRequestHandler<ServiceLimitsQuery, Result<int>>
{
    public Task<Result<int>> Handle(ServiceLimitsQuery request, CancellationToken cancellationToken)
    {
        return request.Interval switch
        {
            ServiceLimitInterval.PerMinute => Task.FromResult(Result.Ok(ServiceLimits.EmailsPerMinute)),
            ServiceLimitInterval.PerHour => Task.FromResult(Result.Ok(ServiceLimits.EmailsPerHour)),
            _ => Task.FromResult(Result.Fail<int>(ErrorMessages.ServiceLimits_InvalidEmailInterval))
        };
    }
}
