using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Newsletters.GroupManagement.Queries;

public class GetAllNewsletterGroupsQueryHandler : IRequestHandler<GetAllNewsletterGroupsQuery, Result<List<NewsletterGroup>>>
{
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;
    public GetAllNewsletterGroupsQueryHandler(IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _newsletterGroupRepository = newsletterGroupRepository;
    }

    public async Task<Result<List<NewsletterGroup>>> Handle(GetAllNewsletterGroupsQuery request, CancellationToken cancellationToken)
    {
        return await _newsletterGroupRepository.GetAllAsync(cancellationToken: cancellationToken);
    }
}
