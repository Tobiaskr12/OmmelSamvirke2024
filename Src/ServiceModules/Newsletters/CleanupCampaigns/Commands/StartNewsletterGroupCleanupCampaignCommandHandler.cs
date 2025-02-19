using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.CleanupCampaigns.Commands;

[UsedImplicitly]
public class StartNewsletterGroupCleanupCampaignCommandValidator : AbstractValidator<StartNewsletterGroupCleanupCampaignCommand>
{
    public StartNewsletterGroupCleanupCampaignCommandValidator(IValidator<NewsletterGroupsCleanupCampaign> validator)
    {
        RuleFor(x => x.CleanupCampaign).SetValidator(validator);
    }
}

public class StartNewsletterGroupCleanupCampaignCommandHandler
    : IRequestHandler<StartNewsletterGroupCleanupCampaignCommand, Result<NewsletterGroupsCleanupCampaign>>
{
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;

    public StartNewsletterGroupCleanupCampaignCommandHandler(
        IRepository<NewsletterGroupsCleanupCampaign> cleanupCampaignRepository,
        IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _cleanupCampaignRepository = cleanupCampaignRepository;
        _newsletterGroupRepository = newsletterGroupRepository;
    }

    public async Task<Result<NewsletterGroupsCleanupCampaign>> Handle(
        StartNewsletterGroupCleanupCampaignCommand request,
        CancellationToken cancellationToken)
    {
        // Check for overlapping campaigns.
        DateTime requestCampaignEnd = request.CleanupCampaign.CampaignStart.AddMonths(request.CleanupCampaign.CampaignDurationMonths);
        Result<List<NewsletterGroupsCleanupCampaign>> overlappingCampaignsResult = await _cleanupCampaignRepository.FindAsync(
            x => x.CampaignStart < requestCampaignEnd &&
                 request.CleanupCampaign.CampaignStart < x.CampaignStart.AddMonths(x.CampaignDurationMonths) &&
                 x.Id != request.CleanupCampaign.Id,
            cancellationToken: cancellationToken);

        if (overlappingCampaignsResult.IsFailed)  return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        if (overlappingCampaignsResult.Value.Count != 0) return Result.Fail(ErrorMessages.NewsletterGroupsCleanupCampaign_OverlappingCampaigns);

        // Retrieve all newsletter groups.
        Result<List<NewsletterGroup>> newsletterGroupsResult = await _newsletterGroupRepository.GetAllAsync(
            cancellationToken: cancellationToken);
        if (newsletterGroupsResult.IsFailed)
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        List<NewsletterGroup>? newsletterGroups = newsletterGroupsResult.Value;
        if (newsletterGroups.Count == 0) return Result.Fail(ErrorMessages.NewsletterGroupsCleanupCampaign_NoNewsletterGroups);

        // Extract distinct newsletter subscribers.
        List<Recipient> distinctNewsletterSubscribers = newsletterGroups
                                                            .Select(g => g.ContactList)
                                                            .SelectMany(cl => cl.Contacts)
                                                            .DistinctBy(r => r.EmailAddress)
                                                            .ToList();

        // Populate the uncleaned recipients list excluding any already cleaned.
        request.CleanupCampaign.UncleanedRecipients.Clear();
        request.CleanupCampaign.UncleanedRecipients = distinctNewsletterSubscribers;
        foreach (Recipient cleanedRecipient in request.CleanupCampaign.CleanedRecipients)
        {
            request.CleanupCampaign.UncleanedRecipients.RemoveAll(r =>
                r.EmailAddress.Equals(cleanedRecipient.EmailAddress, StringComparison.OrdinalIgnoreCase));
        }

        // Save the cleanup campaign.
        Result<NewsletterGroupsCleanupCampaign> saveResult = await _cleanupCampaignRepository.AddAsync(request.CleanupCampaign, cancellationToken);
        return saveResult.IsSuccess
            ? Result.Ok(saveResult.Value)
            : Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}
