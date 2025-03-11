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
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.Subscriptions.Commands;

public class UnsubscribeFromNewslettersCommandHandler  : IRequestHandler<UnsubscribeFromNewslettersCommand, Result>
{
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;
    private readonly IRepository<Recipient> _recipientRepository;
    private readonly IRepository<NewsletterUnsubscribeConfirmation> _unsubscribeRepository;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly IMediator _mediator;

    public UnsubscribeFromNewslettersCommandHandler(
        IRepository<NewsletterGroup> newsletterGroupRepository,
        IRepository<Recipient> recipientRepository,
        IRepository<NewsletterUnsubscribeConfirmation> unsubscribeRepository,
        IEmailTemplateEngine templateEngine,
        IMediator mediator)
    {
        _newsletterGroupRepository = newsletterGroupRepository;
        _recipientRepository = recipientRepository;
        _unsubscribeRepository = unsubscribeRepository;
        _templateEngine = templateEngine;
        _mediator = mediator;
    }

    public async Task<Result> Handle(UnsubscribeFromNewslettersCommand request, CancellationToken cancellationToken)
    {
        // Get all groups
        Result<List<NewsletterGroup>> allGroupsResult = await _newsletterGroupRepository.GetAllAsync(readOnly: false, cancellationToken: cancellationToken);
        if (allGroupsResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        
        // Filter to get requested groups
        List<NewsletterGroup>? allGroups = allGroupsResult.Value;
        List<NewsletterGroup> requestedGroups = allGroups.Where(g => request.NewsletterGroupIds.Contains(g.Id)).ToList();

        string normalizedEmail = request.EmailAddress.ToUpper();
        Result<List<Recipient>> existingRecipientResult = await _recipientRepository.FindAsync(
            x => x.EmailAddress.ToUpper() == normalizedEmail,
            readOnly: false,
            cancellationToken: cancellationToken);

        if (existingRecipientResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        // If no recipient found, user isn't subscribed to anything
        if (existingRecipientResult.Value.Count == 0) return Result.Ok();
        Recipient recipient = existingRecipientResult.Value.First();

        // Filter only those groups where the recipient is actually subscribed
        requestedGroups = requestedGroups
            .Where(g => g.ContactList.Contacts.Any(r => 
                r.EmailAddress.Equals(recipient.EmailAddress, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // If the user is not subscribed to any of the requested groups
        if (requestedGroups.Count == 0) return Result.Fail(ErrorMessages.NewsletterRecipientNotSubscribedToAnything);

        // Check if there's an active unsubscribe request for these groups i.e. not confirmed and not expired
        Result<List<NewsletterUnsubscribeConfirmation>> activeUnsubscribeResult = await _unsubscribeRepository.FindAsync(
            x => x.Recipient.EmailAddress == normalizedEmail
                 && !x.IsConfirmed
                 && x.ConfirmationExpiry > DateTime.UtcNow,
            readOnly: false,
            cancellationToken: cancellationToken);

        if (activeUnsubscribeResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        List<NewsletterUnsubscribeConfirmation>? activeUnsubscribes = activeUnsubscribeResult.Value;

        // Remove any groups that already have an active unsubscribe token
        requestedGroups = requestedGroups.Where(g => 
            !activeUnsubscribes.Any(u => u.NewsletterGroups.Any(ng => ng.Id == g.Id))
        ).ToList();

        // If everything is filtered out
        if (requestedGroups.Count == 0) return Result.Fail(ErrorMessages.NewletterUnsubscribedNoResultAfterFilteringActiveRequests);

        // Create a single unsubscribe confirmation entity
        var unsubscribeConfirmation = new NewsletterUnsubscribeConfirmation
        {
            Recipient = recipient,
            NewsletterGroups = requestedGroups
        };

        Result<NewsletterUnsubscribeConfirmation> addResult = await _unsubscribeRepository.AddAsync(unsubscribeConfirmation, cancellationToken);
        if (addResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        // Generate an unsubscribe link
        string unsubscribeLink = $"http://localhost:5114/confirm-unsubscribe?token={unsubscribeConfirmation.ConfirmationToken}";

        // Generate email bodies
        Result templateResult = _templateEngine.GenerateBodiesFromTemplate(
            Templates.Newsletters.ConfirmNewsletterUnsubscription, 
            ("UnsubscribeConfirmationLink", unsubscribeLink)
        );
        if (templateResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        // Send the email
        var email = new Email
        {
            Subject = _templateEngine.GetSubject(),
            SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
            HtmlBody = _templateEngine.GetHtmlBody(),
            PlainTextBody = _templateEngine.GetPlainTextBody(),
            Recipients = [ new Recipient { EmailAddress = recipient.EmailAddress } ],
            Attachments = []
        };

        Result<EmailSendingStatus> sendResult = await _mediator.Send(new SendEmailCommand(email), cancellationToken);
        return sendResult.IsFailed 
            ? Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok();
    }
}
