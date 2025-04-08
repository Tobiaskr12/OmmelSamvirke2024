using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;
using ServiceModules.Events.IcsFeed;

namespace ServiceModules.Events.Events.Commands;

[UsedImplicitly]
public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator(IValidator<Event> eventValidator)
    {
        RuleFor(x => x.Event).SetValidator(eventValidator);
    }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result<Event>>
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IcsFeedService _icsFeedService;

    public UpdateEventCommandHandler(IRepository<Event> eventRepository, IcsFeedService icsFeedService)
    {
        _eventRepository = eventRepository;
        _icsFeedService = icsFeedService;
    }

    public async Task<Result<Event>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        Result<Event> updateResult = await _eventRepository.UpdateAsync(request.Event, cancellationToken);
        
        if (updateResult.IsSuccess)
        {
            // Update the ICS feed
            await _icsFeedService.UpdateCalendarFile(cancellationToken);
        }
        
        return updateResult.IsFailed 
            ? Result.Fail<Event>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : updateResult.Value;
    }
}
