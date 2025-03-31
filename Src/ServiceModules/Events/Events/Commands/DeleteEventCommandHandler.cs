using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.Events.Commands;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result>
{
    private readonly IRepository<Event> _eventRepository;

    public DeleteEventCommandHandler(IRepository<Event> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        Result<Event> eventResult = await _eventRepository.GetByIdAsync(
            request.Id,
            readOnly: false,
            cancellationToken: cancellationToken);
        if (eventResult.IsFailed || eventResult.Value is null) return Result.Fail(ErrorMessages.GenericNotFound);
        
        return await _eventRepository.DeleteAsync(eventResult.Value, cancellationToken);
    }
}
