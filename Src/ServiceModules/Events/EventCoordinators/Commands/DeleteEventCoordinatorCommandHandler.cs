using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.EventCoordinators.Commands;

public class DeleteEventCoordinatorCommandHandler : IRequestHandler<DeleteEventCoordinatorCommand, Result>
{
    private readonly IRepository<EventCoordinator> _coordinatorRepository;
    public DeleteEventCoordinatorCommandHandler(IRepository<EventCoordinator> coordinatorRepository)
    {
        _coordinatorRepository = coordinatorRepository;
    }
    
    public async Task<Result> Handle(DeleteEventCoordinatorCommand request, CancellationToken cancellationToken)
    {
        Result<EventCoordinator> coordinatorResult = await _coordinatorRepository.GetByIdAsync(
            request.Id,
            readOnly: false,
            cancellationToken: cancellationToken);
        if (coordinatorResult.IsFailed || coordinatorResult.Value is null) return Result.Fail(ErrorMessages.GenericNotFound);
        
        return await _coordinatorRepository.DeleteAsync(coordinatorResult.Value, cancellationToken);
    }
}
