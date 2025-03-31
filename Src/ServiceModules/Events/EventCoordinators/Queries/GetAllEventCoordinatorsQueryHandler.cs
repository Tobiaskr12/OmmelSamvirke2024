using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.EventCoordinators.Queries;

public class GetAllEventCoordinatorsQueryHandler : IRequestHandler<GetAllEventCoordinatorsQuery, Result<List<EventCoordinator>>>
{
    private readonly IRepository<EventCoordinator> _coordinatorRepository;
    public GetAllEventCoordinatorsQueryHandler(IRepository<EventCoordinator> coordinatorRepository)
    {
        _coordinatorRepository = coordinatorRepository;
    }
    
    public async Task<Result<List<EventCoordinator>>> Handle(GetAllEventCoordinatorsQuery request, CancellationToken cancellationToken)
    {
        Result<List<EventCoordinator>> allCoordinatorsQuery = await _coordinatorRepository.GetAllAsync(cancellationToken: cancellationToken);

        return allCoordinatorsQuery.IsFailed
            ? Result.Fail<List<EventCoordinator>>(ErrorMessages.GenericErrorWithRetryPrompt)
            : allCoordinatorsQuery.Value;
    }
}