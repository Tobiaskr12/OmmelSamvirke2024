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
    private readonly IRepository<ContactList> _contactListRepository;

    public StartNewsletterGroupCleanupCampaignCommandHandler(
        IRepository<NewsletterGroupsCleanupCampaign> cleanupCampaignRepository,
        IRepository<NewsletterGroup> newsletterGroupRepository,
        IRepository<ContactList> contactListRepository)
    {
        _cleanupCampaignRepository = cleanupCampaignRepository;
        _newsletterGroupRepository = newsletterGroupRepository;
        _contactListRepository = contactListRepository;
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

        // Retrieve relevant contact lists
        IEnumerable<int> contactListIds = newsletterGroups.Select(x => x.ContactList.Id);
        Result<List<ContactList>> contactListsQuery =
            await _contactListRepository.FindAsync(
                x => contactListIds.Contains(x.Id),
                readOnly: false,
                cancellationToken: cancellationToken
            );

        if (contactListsQuery.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        
        // Extract distinct newsletter subscribers.
        List<Recipient> distinctNewsletterSubscribers = 
            contactListsQuery.Value
                .SelectMany(g => g.Contacts)
                .DistinctBy(r => r.EmailAddress)
                .ToList();

        // Populate the uncleaned recipients list
        request.CleanupCampaign.UnconfirmedRecipients = distinctNewsletterSubscribers;
        
        // Save the cleanup campaign.
        Result<NewsletterGroupsCleanupCampaign> saveResult = await _cleanupCampaignRepository.AddAsync(request.CleanupCampaign, cancellationToken);
        return saveResult.IsSuccess
            ? Result.Ok(saveResult.Value)
            : Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}
