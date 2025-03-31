using System.Linq.Expressions;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Events.EventCoordinators;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Events.EventCoordinators.Queries;

public class GetEventCoordinatorsByNameQueryHandler : IRequestHandler<GetEventCoordinatorsByNameQuery, Result<List<EventCoordinator>>>
{
    private readonly IRepository<EventCoordinator> _coordinatorRepository;
    public GetEventCoordinatorsByNameQueryHandler(IRepository<EventCoordinator> coordinatorRepository)
    {
        _coordinatorRepository = coordinatorRepository;
    }
    
    public async Task<Result<List<EventCoordinator>>> Handle(GetEventCoordinatorsByNameQuery request, CancellationToken cancellationToken)
    {
        Result<List<EventCoordinator>> allCoordinatorsQuery = await _coordinatorRepository.GetAllAsync(cancellationToken: cancellationToken);

        return allCoordinatorsQuery.IsFailed 
            ? Result.Fail<List<EventCoordinator>>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : allCoordinatorsQuery.Value.Where(x => x.Name.Contains(request.NameSearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
