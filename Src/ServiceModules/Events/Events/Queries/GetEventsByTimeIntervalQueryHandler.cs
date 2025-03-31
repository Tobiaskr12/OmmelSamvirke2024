using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.Events.Queries;

public class GetEventsByTimeIntervalQueryHandler : IRequestHandler<GetEventsByTimeIntervalQuery, Result<List<Event>>>
{
    private readonly IRepository<Event> _eventRepository;

    public GetEventsByTimeIntervalQueryHandler(IRepository<Event> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<List<Event>>> Handle(GetEventsByTimeIntervalQuery request, CancellationToken cancellationToken)
    {
        Result<List<Event>> queryResult = await _eventRepository.FindAsync(
            e => (e.StartTime >= request.StartTime && e.StartTime <= request.EndTime) || (e.EndTime >= request.StartTime && e.EndTime <= request.EndTime) ,
            readOnly: true,
            cancellationToken: cancellationToken
        );

        if (queryResult.IsFailed || queryResult.Value is null) return Result.Fail<List<Event>>(ErrorMessages.GenericErrorWithRetryPrompt);
        
        return Result.Ok(queryResult.Value);
    }
}
