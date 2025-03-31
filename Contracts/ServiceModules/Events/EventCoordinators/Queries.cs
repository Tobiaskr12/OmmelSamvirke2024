using DomainModules.Events.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Events.EventCoordinators;

public record GetEventCoordinatorsByNameQuery(string NameSearchTerm) : IRequest<Result<List<EventCoordinator>>>;
public record GetAllEventCoordinatorsQuery() : IRequest<Result<List<EventCoordinator>>>;
