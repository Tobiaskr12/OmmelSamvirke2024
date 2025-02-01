using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;
using OmmelSamvirke.ServiceModules.Errors;
using Exception = System.Exception;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

public record UnsubscribeFromContactListCommand(string EmailAddress, Guid UnsubscribeToken) : IRequest<Result>;

public class UnsubscribeFromContactListCommandHandler : IRequestHandler<UnsubscribeFromContactListCommand, Result>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateEngine _emailTemplateEngine;
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<ContactListUnsubscription> _contactListUnsubscriptionRepository;

    public UnsubscribeFromContactListCommandHandler(
        ILogger logger, 
        IMediator mediator,
        IEmailTemplateEngine emailTemplateEngine,
        IRepository<ContactList> contactListRepository,
        IRepository<ContactListUnsubscription> contactListUnsubscriptionRepository)
    {
        _logger = logger;
        _mediator = mediator;
        _emailTemplateEngine = emailTemplateEngine;
        _contactListRepository = contactListRepository;
        _contactListUnsubscriptionRepository = contactListUnsubscriptionRepository;
    }
    
    public async Task<Result> Handle(UnsubscribeFromContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Result<List<ContactList>> contactListQueryResult = await _contactListRepository.FindAsync(
                x => x.UnsubscribeToken == request.UnsubscribeToken,
                readOnly: false,
                cancellationToken: cancellationToken
            );

            if (contactListQueryResult.IsFailed || contactListQueryResult.Value.Count == 0)
            {
                return Result.Fail(ErrorMessages.ContactList_UnsubscribeTokenEmptyQuery);
            }

            if (contactListQueryResult.Value.Count > 1)
            {
                _logger.LogWarning(
                    "Multiple contact lists with the same unsubscribe token were found in {command}. Total count: {count}",
                    nameof(UnsubscribeFromContactListCommand),
                    contactListQueryResult.Value.Count
                );
            }

            ContactList contactList = contactListQueryResult.Value.First();
            
            // Remove all matching recipients.
            List<Recipient> recipients = contactList.Contacts.Where(x => x.EmailAddress == request.EmailAddress).ToList();
            foreach (Recipient recipient in recipients)
            {
                contactList.Contacts.Remove(recipient);
            }

            Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(contactList, cancellationToken);
            if (updateResult.IsSuccess)
            {
                bool recipientStillInContactList = updateResult.Value.Contacts.Any(x => x.EmailAddress == request.EmailAddress);
                if (recipientStillInContactList)
                {
                    return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                }

                // Create a new unsubscription record with the email and contact list id.
                var unsubscription = new ContactListUnsubscription
                {
                    EmailAddress = request.EmailAddress,
                    ContactListId = contactList.Id
                };

                Result<ContactListUnsubscription> unsubscriptionResult = await _contactListUnsubscriptionRepository.AddAsync(unsubscription, cancellationToken);
                if (unsubscriptionResult.IsFailed)
                {
                    return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
                }

                // Generate email receipt using the new UndoToken.
                Result emailTemplateResult = _emailTemplateEngine.GenerateBodiesFromTemplate(
                    "Empty.html", // TODO - Use real template
                    parameters: ("UndoToken", unsubscriptionResult.Value.UndoToken.ToString())
                );
                if (emailTemplateResult.IsFailed)
                {
                    throw new Exception("Email body generation failed.");
                }

                var emailRecipient = new Recipient { EmailAddress = request.EmailAddress };

                await _mediator.Send(new SendEmailCommand(new Email
                {
                    SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                    Recipients = [emailRecipient],
                    Attachments = [],
                    Subject = _emailTemplateEngine.GetSubject(),
                    HtmlBody = _emailTemplateEngine.GetHtmlBody(),
                    PlainTextBody = _emailTemplateEngine.GetPlainTextBody()
                }), cancellationToken);

                return Result.Ok();
            }

            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        catch (Exception ex)
        {
            _logger.LogError("{errorMessage}", ex.Message);
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
    }
}