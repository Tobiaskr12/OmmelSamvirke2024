using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.Events.Queries;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Result<Event>>
{
    private readonly IRepository<Event> _eventRepository;

    public GetEventByIdQueryHandler(IRepository<Event> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<Event>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        Result<Event> eventResult = await _eventRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
        if (eventResult.IsFailed || eventResult.Value is null) return Result.Fail<Event>(ErrorMessages.GenericNotFound);
        
        return eventResult.Value;
    }
}
