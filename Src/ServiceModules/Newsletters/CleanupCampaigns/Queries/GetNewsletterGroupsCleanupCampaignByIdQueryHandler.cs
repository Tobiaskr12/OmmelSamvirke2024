using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Newsletters.CleanupCampaigns.Queries;

public class GetNewsletterGroupsCleanupCampaignByIdQueryHandler : IRequestHandler<GetNewsletterGroupsCleanupCampaignByIdQuery, Result<NewsletterGroupsCleanupCampaign>>
{
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _campaignRepository;

    public GetNewsletterGroupsCleanupCampaignByIdQueryHandler(IRepository<NewsletterGroupsCleanupCampaign> campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }
    
    public async Task<Result<NewsletterGroupsCleanupCampaign>> Handle(GetNewsletterGroupsCleanupCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        return await _campaignRepository.GetByIdAsync(request.Id, readOnly: request.ReadOnly, cancellationToken: cancellationToken);
    }
}
