using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Newsletters.Queries;

public class GetPaginatedNewslettersQueryHandler : IRequestHandler<GetPaginatedNewslettersQuery, Result<PaginatedResult<Email>>>
{
    private readonly IRepository<Email> _emailRepository;
    public GetPaginatedNewslettersQueryHandler(IRepository<Email> emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<Result<PaginatedResult<Email>>> Handle(GetPaginatedNewslettersQuery request, CancellationToken cancellationToken)
    {
        return await _emailRepository.GetPaginatedAsync(request.Page, request.PageSize, cancellationToken: cancellationToken);
    }
}
