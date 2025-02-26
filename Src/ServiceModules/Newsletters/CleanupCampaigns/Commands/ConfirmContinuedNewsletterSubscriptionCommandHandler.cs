using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.CleanupCampaigns.Commands;

public class ConfirmContinuedNewsletterSubscriptionCommandHandler : IRequestHandler<ConfirmContinuedNewsletterSubscriptionCommand, Result>
{
    private readonly IRepository<Recipient> _recipientRepository;
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;
    private readonly ILoggingHandler _logger;

    public ConfirmContinuedNewsletterSubscriptionCommandHandler(
        IRepository<Recipient> recipientRepository,
        IRepository<NewsletterGroupsCleanupCampaign> cleanupCampaignRepository,
        ILoggingHandler logger)
    {
        _recipientRepository = recipientRepository;
        _cleanupCampaignRepository = cleanupCampaignRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmContinuedNewsletterSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Find the recipient by token.
        Result<List<Recipient>> recipientQuery = await _recipientRepository.FindAsync(
            x => x.Token == request.RecipientToken,
            cancellationToken: cancellationToken);

        if (recipientQuery.IsFailed || recipientQuery.Value.Count == 0)
        {
            return Result.Fail(ErrorMessages.NewsletterSubscriberNotFound);
        }

        if (recipientQuery.Value.Count > 1)
        {
            _logger.LogWarning($"Multiple recipients found with token {request.RecipientToken}. Using the first match.");
        }

        Recipient recipient = recipientQuery.Value.First();

        // Retrieve the active campaign (where current time is between start and end, and campaign is started).
        DateTime now = DateTime.UtcNow;
        Result<List<NewsletterGroupsCleanupCampaign>> campaignQuery = await _cleanupCampaignRepository.FindAsync(
            x => x.CampaignStart <= now &&
                 now <= x.CampaignStart.AddMonths(x.CampaignDurationMonths) &&
                 x.IsCampaignStarted,
            readOnly: false,
            cancellationToken: cancellationToken);

        if (campaignQuery.IsFailed || campaignQuery.Value.Count == 0)
        {
            return Result.Fail(ErrorMessages.NoActiveCleanupCampaign);
        }

        if (campaignQuery.Value.Count > 1)
        {
            _logger.LogWarning("Multiple active newsletter cleanup campaigns found. Using the first active campaign.");
        }

        NewsletterGroupsCleanupCampaign cleanupCampaign = campaignQuery.Value.First();

        // Find the recipient in the uncleaned list.
        Recipient? campaignRecipient = cleanupCampaign.UnconfirmedRecipients
                                                      .FirstOrDefault(x => x.Token == recipient.Token);

        // Remove recipient from the unconfirmed list
        cleanupCampaign.UnconfirmedRecipients.Remove(campaignRecipient);
        Result<NewsletterGroupsCleanupCampaign> updateResult = await _cleanupCampaignRepository.UpdateAsync(cleanupCampaign, cancellationToken);

        return updateResult.IsSuccess
            ? Result.Ok()
            : Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}