using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.Queries;

public class GetPaginatedEmailsQueryHandler : IRequestHandler<GetPaginatedEmailsQuery, Result<PaginatedResult<Email>>>
{
    private readonly IRepository<Email> _emailRepository;
    public GetPaginatedEmailsQueryHandler(IRepository<Email> emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<Result<PaginatedResult<Email>>> Handle(GetPaginatedEmailsQuery request, CancellationToken cancellationToken)
    {
        return await _emailRepository.GetPaginatedAsync(request.Page, request.PageSize, cancellationToken: cancellationToken);
    }
}
