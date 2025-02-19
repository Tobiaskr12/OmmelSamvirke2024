using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;
namespace ServiceModules.Newsletters.CleanupCampaigns.Commands;

public class DeleteCleanupCampaignCommandHandler : IRequestHandler<DeleteCleanupCampaignCommand, Result>
{
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;

    public DeleteCleanupCampaignCommandHandler(IRepository<NewsletterGroupsCleanupCampaign> cleanupCampaignRepository)
    {
        _cleanupCampaignRepository = cleanupCampaignRepository;
    }

    public async Task<Result> Handle(DeleteCleanupCampaignCommand request, CancellationToken cancellationToken)
    {
        Result<NewsletterGroupsCleanupCampaign> campaignQuery = await _cleanupCampaignRepository.GetByIdAsync(
            request.CleanupCampaignId,
            readOnly: false,
            cancellationToken: cancellationToken);

        if (campaignQuery.IsFailed || campaignQuery.Value is null)
        {
            return Result.Fail(ErrorMessages.NoActiveCleanupCampaign);
        }

        NewsletterGroupsCleanupCampaign campaign = campaignQuery.Value;

        if (campaign.IsCampaignStarted)
        {
            return Result.Fail(ErrorMessages.CleanupCampaign_CannotDeleteActiveCampaign);
        }

        Result deleteResult = await _cleanupCampaignRepository.DeleteAsync(campaign, cancellationToken);
        return deleteResult.IsSuccess
            ? Result.Ok()
            : Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}
