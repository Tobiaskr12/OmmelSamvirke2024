using Contracts.DataAccess;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters;

public record GetNewsletterByIdQuery(int Id)  : IRequest<Result<Email>>;
public record GetPaginatedNewslettersQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PaginatedResult<Email>>>;
