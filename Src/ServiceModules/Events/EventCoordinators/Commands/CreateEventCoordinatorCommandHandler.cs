using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Events.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.EventCoordinators.Commands;

[UsedImplicitly]
public class CreateEventCoordinatorCommandValidator : AbstractValidator<CreateEventCoordinatorCommand>
{
    public CreateEventCoordinatorCommandValidator(IValidator<EventCoordinator> eventCoordinatorValidator)
    {
        RuleFor(x => x.EventCoordinator).SetValidator(eventCoordinatorValidator);
    }
}

public class CreateEventCoordinatorCommandHandler : IRequestHandler<CreateEventCoordinatorCommand, Result<EventCoordinator>>
{
    private readonly IRepository<EventCoordinator> _coordinatorRepository;
    public CreateEventCoordinatorCommandHandler(IRepository<EventCoordinator> coordinatorRepository)
    {
        _coordinatorRepository = coordinatorRepository;
    }
    
    public async Task<Result<EventCoordinator>> Handle(CreateEventCoordinatorCommand request, CancellationToken cancellationToken)
    {
        Result<EventCoordinator> result = await _coordinatorRepository.AddAsync(request.EventCoordinator, cancellationToken);
        return result.IsFailed 
            ? Result.Fail<EventCoordinator>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : result.Value;
    }
}
