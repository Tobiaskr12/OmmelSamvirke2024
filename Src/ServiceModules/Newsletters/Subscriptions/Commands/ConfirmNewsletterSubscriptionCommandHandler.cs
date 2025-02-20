using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.Subscriptions.Commands;

public class ConfirmNewsletterSubscriptionCommandHandler : IRequestHandler<ConfirmNewsletterSubscriptionCommand, Result>
{
    private readonly IRepository<NewsletterSubscriptionConfirmation> _subscriptionRepository;
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;

    public ConfirmNewsletterSubscriptionCommandHandler(
        IRepository<NewsletterSubscriptionConfirmation> subscriptionRepository,
        IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _newsletterGroupRepository = newsletterGroupRepository;
    }

    public async Task<Result> Handle(ConfirmNewsletterSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Find the subscription confirmation by token.
        Result<List<NewsletterSubscriptionConfirmation>> findResult =
            await _subscriptionRepository.FindAsync(
                x => x.ConfirmationToken == request.Token,
                readOnly: false,
                cancellationToken: cancellationToken);

        if (findResult.IsFailed || findResult.Value.Count == 0)
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        NewsletterSubscriptionConfirmation confirmation = findResult.Value.First();

        // Check if it's expired or already confirmed.
        if (confirmation.ConfirmationExpiry < DateTime.UtcNow) return Result.Fail(ErrorMessages.NewsletterConfirmationTokenExpired);
        if (confirmation.IsConfirmed) return Result.Fail(ErrorMessages.NewsletterConfirmationTokenAlreadyConfirmed);

        // Mark as confirmed.
        confirmation.IsConfirmed = true;
        confirmation.ConfirmationTime = DateTime.UtcNow;

        // For each group in the subscription, add the recipient to the group’s contact list if not already present.
        foreach (NewsletterGroup group in confirmation.NewsletterGroups)
        {
            ContactList contactList = group.ContactList;
            if (!contactList.Contacts.Any(r => r.EmailAddress.Equals(confirmation.Recipient.EmailAddress, StringComparison.OrdinalIgnoreCase)))
            {
                contactList.Contacts.Add(confirmation.Recipient);
            }
        }

        // Update the subscription confirmation.
        Result<NewsletterSubscriptionConfirmation> updateConfirmationResult = await _subscriptionRepository.UpdateAsync(confirmation, cancellationToken);
        if (updateConfirmationResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        // Optionally update each NewsletterGroup if your data layer requires explicit updates.
        Result<List<NewsletterGroup>> updateResult = await _newsletterGroupRepository.UpdateAsync(
            confirmation.NewsletterGroups,
            cancellationToken: cancellationToken
        );
        
        return updateResult.IsFailed 
            ? Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok();
    }
}
