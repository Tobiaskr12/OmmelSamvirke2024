using Contracts.DataAccess.Emails.Enums;
using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.Queries;

public record ServiceLimitsQuery(ServiceLimitInterval Interval) : IRequest<Result<int>>;

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
