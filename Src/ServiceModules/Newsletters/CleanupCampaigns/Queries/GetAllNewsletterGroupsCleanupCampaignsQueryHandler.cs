using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Newsletters.CleanupCampaigns.Queries;

public class GetAllNewsletterGroupsCleanupCampaignsQueryHandler 
    : IRequestHandler<GetAllNewsletterGroupsCleanupCampaignsQuery, Result<List<NewsletterGroupsCleanupCampaign>>>
{
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _campaignRepository;

    public GetAllNewsletterGroupsCleanupCampaignsQueryHandler(IRepository<NewsletterGroupsCleanupCampaign> campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }
    
    public async Task<Result<List<NewsletterGroupsCleanupCampaign>>> Handle(
        GetAllNewsletterGroupsCleanupCampaignsQuery request, 
        CancellationToken cancellationToken)
    {
        return await _campaignRepository.GetAllAsync(cancellationToken: cancellationToken);
    }
}
