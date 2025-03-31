using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using Contracts.ServiceModules.Events.Events;
using DomainModules.Events.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

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

    public UpdateEventCommandHandler(IRepository<Event> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<Event>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        Result<Event> updateResult = await _eventRepository.UpdateAsync(request.Event, cancellationToken);
        
        return updateResult.IsFailed 
            ? Result.Fail<Event>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : updateResult.Value;
    }
}
