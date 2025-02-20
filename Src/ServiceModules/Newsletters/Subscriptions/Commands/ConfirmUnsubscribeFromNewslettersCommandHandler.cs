using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.Subscriptions.Commands;

public class ConfirmUnsubscribeFromNewslettersCommandHandler 
    : IRequestHandler<ConfirmUnsubscribeFromNewslettersCommand, Result>
{
    private readonly IRepository<NewsletterUnsubscribeConfirmation> _unsubscribeRepository;
    private readonly IRepository<NewsletterSubscriptionConfirmation> _subscriptionRepository;
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;

    public ConfirmUnsubscribeFromNewslettersCommandHandler(
        IRepository<NewsletterUnsubscribeConfirmation> unsubscribeRepository,
        IRepository<NewsletterSubscriptionConfirmation> subscriptionRepository,
        IRepository<NewsletterGroupsCleanupCampaign> cleanupCampaignRepository,
        IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _unsubscribeRepository = unsubscribeRepository;
        _subscriptionRepository = subscriptionRepository;
        _cleanupCampaignRepository = cleanupCampaignRepository;
        _newsletterGroupRepository = newsletterGroupRepository;
    }

    public async Task<Result> Handle(ConfirmUnsubscribeFromNewslettersCommand request, CancellationToken cancellationToken)
    {
        // Find the unsubscribe confirmation by token
        Result<List<NewsletterUnsubscribeConfirmation>> findResult = await _unsubscribeRepository.FindAsync(
            x => x.ConfirmationToken == request.Token,
            readOnly: false,
            cancellationToken: cancellationToken);

        if (findResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        if (findResult.Value.Count == 0) return Result.Fail(ErrorMessages.NewsletterUnsubscribeTokenInvalid);

        NewsletterUnsubscribeConfirmation unsubscribe = findResult.Value.First();

        // Check if it's expired or already confirmed
        if (unsubscribe.ConfirmationExpiry < DateTime.UtcNow) return Result.Fail(ErrorMessages.NewsletterUnsubscribeTokenExpired);

        if (unsubscribe.IsConfirmed) return Result.Fail(ErrorMessages.NewsletterUnsubscribeTokenAlreadyUsed);

        // For each group in the unsubscribe entity, remove the user from the contact list
        string recipientEmail = unsubscribe.Recipient.EmailAddress;
        foreach (NewsletterGroup group in unsubscribe.NewsletterGroups)
        {
            ContactList contactList = group.ContactList;
            Recipient? toRemove = contactList.Contacts.FirstOrDefault(r => r.EmailAddress.Equals(recipientEmail, StringComparison.OrdinalIgnoreCase));
            if (toRemove is not null) contactList.Contacts.Remove(toRemove);
        }

        // Delete the old subscription confirmations for these groups
        List<int> groupIds = unsubscribe.NewsletterGroups.Select(g => g.Id).ToList();
        Result<List<NewsletterSubscriptionConfirmation>> subConfirmFind = await _subscriptionRepository.FindAsync(
            sc => sc.Recipient.EmailAddress.Equals(recipientEmail, StringComparison.OrdinalIgnoreCase)
                  && sc.NewsletterGroups.Any(ng => groupIds.Contains(ng.Id)),
            readOnly: false,
            cancellationToken: cancellationToken);

        if (subConfirmFind.IsSuccess && subConfirmFind.Value.Count != 0)
        {
            foreach (NewsletterSubscriptionConfirmation subConf in subConfirmFind.Value)
            {
                await _subscriptionRepository.DeleteAsync(subConf, cancellationToken);
            }
        }

        // Mark the unsubscribe confirmation as confirmed
        unsubscribe.IsConfirmed = true;
        unsubscribe.ConfirmationTime = DateTime.UtcNow;

        Result<NewsletterUnsubscribeConfirmation> updateUnsubscribeResult = await _unsubscribeRepository.UpdateAsync(unsubscribe, cancellationToken);
        if (updateUnsubscribeResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        Result<List<NewsletterGroup>> updateResult = await _newsletterGroupRepository.UpdateAsync(
            unsubscribe.NewsletterGroups,
            cancellationToken: cancellationToken);
        
        if (updateResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        
        // Check if the user is registered in a cleanup campaign. If they are, remove them
        Result<List<NewsletterGroupsCleanupCampaign>> activeCleanupCampaignsQuery = await _cleanupCampaignRepository.FindAsync(x => 
                x.CampaignStart < DateTime.UtcNow &&
                DateTime.UtcNow < x.CampaignStart.AddMonths(x.CampaignDurationMonths) && 
                x.IsCampaignStarted,
            readOnly: false,
            cancellationToken);

        // We ignore failures from here. The user should not have to perform the request again if this step fails
        if (activeCleanupCampaignsQuery.IsSuccess)
        {
            List<NewsletterGroupsCleanupCampaign>? campaigns = activeCleanupCampaignsQuery.Value;
            foreach (NewsletterGroupsCleanupCampaign campaign in campaigns)
            {
                Recipient? campaignRecipient = campaign.UncleanedRecipients.FirstOrDefault(x => 
                    x.EmailAddress.Equals(recipientEmail, StringComparison.OrdinalIgnoreCase)
                );
                
                if (campaignRecipient is not null)
                {
                    campaign.UncleanedRecipients.Remove(campaignRecipient);
                    await _cleanupCampaignRepository.UpdateAsync(campaign, cancellationToken);
                }
            }
        }
        
        return Result.Ok();
    }
}
