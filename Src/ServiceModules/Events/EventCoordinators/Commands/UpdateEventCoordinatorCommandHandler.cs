using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.EventCoordinators.Commands;

[UsedImplicitly]
public class UpdateEventCoordinatorCommandValidator : AbstractValidator<UpdateEventCoordinatorCommand>
{
    public UpdateEventCoordinatorCommandValidator(IValidator<EventCoordinator> eventCoordinatorValidator)
    {
        RuleFor(x => x.EventCoordinator).SetValidator(eventCoordinatorValidator);
    }
}

public class UpdateEventCoordinatorCommandHandler : IRequestHandler<UpdateEventCoordinatorCommand, Result<EventCoordinator>>
{
    private readonly IRepository<EventCoordinator> _coordinatorRepository;
    public UpdateEventCoordinatorCommandHandler(IRepository<EventCoordinator> coordinatorRepository)
    {
        _coordinatorRepository = coordinatorRepository;
    }
    
    public async Task<Result<EventCoordinator>> Handle(UpdateEventCoordinatorCommand request, CancellationToken cancellationToken)
    {
        Result<EventCoordinator> result = await _coordinatorRepository.UpdateAsync(request.EventCoordinator, cancellationToken);
        return result.IsFailed 
            ? Result.Fail<EventCoordinator>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : result.Value;
    }
}
