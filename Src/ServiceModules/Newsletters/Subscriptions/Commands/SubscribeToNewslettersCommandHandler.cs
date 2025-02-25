using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Newsletters.Subscriptions.Commands;

public class SubscribeToNewslettersCommandHandler : IRequestHandler<SubscribeToNewslettersCommand, Result>
{
    private readonly IRepository<NewsletterSubscriptionConfirmation> _subscriptionRepository;
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;
    private readonly IRepository<Recipient> _recipientRepository;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly IMediator _mediator;

    public SubscribeToNewslettersCommandHandler(
        IRepository<NewsletterSubscriptionConfirmation> subscriptionRepository,
        IRepository<NewsletterGroup> newsletterGroupRepository,
        IRepository<Recipient> recipientRepository,
        IEmailTemplateEngine templateEngine,
        IMediator mediator)
    {
        _subscriptionRepository = subscriptionRepository;
        _newsletterGroupRepository = newsletterGroupRepository;
        _recipientRepository = recipientRepository;
        _templateEngine = templateEngine;
        _mediator = mediator;
    }

    public async Task<Result> Handle(SubscribeToNewslettersCommand request, CancellationToken cancellationToken)
    {
        // Retrieve all newsletter groups.
        Result<List<NewsletterGroup>> groupsResult = await _newsletterGroupRepository.GetAllAsync(readOnly: false, cancellationToken: cancellationToken);
        if (groupsResult.IsFailed) return Result.Fail("Failed to retrieve newsletter groups.");

        List<NewsletterGroup>? allGroups = groupsResult.Value;
        List<NewsletterGroup> requestedGroups = allGroups.Where(ng => request.NewsletterGroupIds.Contains(ng.Id)).ToList();
        if (requestedGroups.Count == 0) return Result.Fail("No valid newsletter groups found.");

        // Get existing recipient, if any.
        Recipient recipient;
        Result<List<Recipient>> existingRecipientResult = await _recipientRepository.FindAsync(
            x => x.EmailAddress == request.EmailAddress,
            readOnly: false,
            cancellationToken: cancellationToken);

        if (existingRecipientResult.IsSuccess && existingRecipientResult.Value.Count != 0)
        {
            recipient = existingRecipientResult.Value.First();
        }
        else
        {
            recipient = new Recipient { EmailAddress = request.EmailAddress };
        }

        // Filter out newsletter groups where the recipient is already subscribed.
        requestedGroups = requestedGroups.Where(group => group.ContactList.Contacts.All(r => r.EmailAddress != recipient.EmailAddress)).ToList();
        
        if (requestedGroups.Count == 0) return Result.Fail("User is already subscribed to all selected newsletters.");

        // Check for existing pending confirmations.
        Result<List<NewsletterSubscriptionConfirmation>> existingPending = await _subscriptionRepository.FindAsync(
            x => !x.IsConfirmed &&
                 x.ConfirmationExpiry > DateTime.UtcNow &&
                 x.Recipient.EmailAddress == request.EmailAddress,
            readOnly: false,
            cancellationToken: cancellationToken);
        
        if (existingPending.IsSuccess && existingPending.Value.Count >= 5)
            return Result.Fail("Too many pending subscription requests for this email.");

        // Create a new subscription confirmation with associated newsletter groups.
        var confirmation = new NewsletterSubscriptionConfirmation
        {
            Recipient = recipient,
            NewsletterGroups = requestedGroups
        };

        Result<NewsletterSubscriptionConfirmation> addResult = await _subscriptionRepository.AddAsync(confirmation, cancellationToken);
        if (addResult.IsFailed) return Result.Fail("Failed to create subscription request.");

        // Generate confirmation email.
        string confirmationLink = $"https://www.ommelsamvirke.com/confirm-subscription?token={confirmation.ConfirmationToken}";
        Result templateResult = _templateEngine.GenerateBodiesFromTemplate(Templates.Newsletters.ConfirmNewsletterSubscription,
            ("ConfirmationLink", confirmationLink));
        if (templateResult.IsFailed)
            return Result.Fail("Failed to generate confirmation email template.");

        var email = new Email
        {
            Subject = _templateEngine.GetSubject(),
            SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
            Recipients = [new Recipient { EmailAddress = request.EmailAddress }],
            HtmlBody = _templateEngine.GetHtmlBody(),
            PlainTextBody = _templateEngine.GetPlainTextBody(),
            Attachments = []
        };

        Result<EmailSendingStatus> sendResult = await _mediator.Send(new SendEmailCommand(email), cancellationToken);
        return sendResult.IsFailed 
            ? Result.Fail("Failed to send confirmation email.") 
            : Result.Ok();
    }
}
