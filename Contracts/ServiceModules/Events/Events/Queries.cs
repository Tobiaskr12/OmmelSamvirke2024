using Contracts.DataAccess;
using DomainModules.Events.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Events.Events;
public record GetEventByIdQuery(int Id) : IRequest<Result<Event>>;
public record GetEventsByTimeIntervalQuery(DateTime StartTime, DateTime EndTime) : IRequest<Result<List<Event>>>;
