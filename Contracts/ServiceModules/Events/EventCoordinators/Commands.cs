using DomainModules.Events.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Events.EventCoordinators;

public record CreateEventCoordinatorCommand(EventCoordinator EventCoordinator) : IRequest<Result<EventCoordinator>>;
public record UpdateEventCoordinatorCommand(EventCoordinator EventCoordinator) : IRequest<Result<EventCoordinator>>;
public record DeleteEventCoordinatorCommand(int Id) : IRequest<Result>;
