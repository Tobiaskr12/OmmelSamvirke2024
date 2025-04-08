using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;
using ServiceModules.Events.IcsFeed;

namespace ServiceModules.Events.Events.Commands;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IcsFeedService _icsFeedService;

    public DeleteEventCommandHandler(IRepository<Event> eventRepository, IcsFeedService icsFeedService)
    {
        _eventRepository = eventRepository;
        _icsFeedService = icsFeedService;
    }

    public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        Result<Event> eventResult = await _eventRepository.GetByIdAsync(
            request.Id,
            readOnly: false,
            cancellationToken: cancellationToken);
        if (eventResult.IsFailed || eventResult.Value is null) return Result.Fail(ErrorMessages.GenericNotFound);
        
        Result deleteResult = await _eventRepository.DeleteAsync(eventResult.Value, cancellationToken);
        if (deleteResult.IsSuccess)
        {
            // Update the ICS feed
            await _icsFeedService.UpdateCalendarFile(cancellationToken);
        }

        return deleteResult;
    }
}
