using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

// TODO - Research how the resilience of this command can be improved
// Retires, transactions, etc.
public class SendEmailToContactListCommand : IRequest<Result>
{
    public Email Email { get; }
    public ContactList ContactList { get; }
    public int? BatchSize { get; }
    public bool UseBcc { get; }

    public SendEmailToContactListCommand(Email email, ContactList contactList, int? batchSize = null, bool useBcc = false)
    {
        Email = email;
        ContactList = contactList;
        BatchSize = batchSize;
        UseBcc = useBcc;

        // This is needed to pass validator - Will be removed in the handler
        Email.Recipients = [new Recipient { EmailAddress = "fake@example.com" }];
    }
}

[UsedImplicitly]
public class SendEmailToContactListCommandValidator : AbstractValidator<SendEmailToContactListCommand>
{
    public SendEmailToContactListCommandValidator(IValidator<Email> emailValidator, IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);

        RuleFor(x => x.ContactList.Contacts)
            .NotEmpty()
            .WithMessage(ErrorMessages.EmailSendingToContactList_ContactListMustNotBeEmpty);
    }
}

public class SendEmailToContactListCommandHandler : IRequestHandler<SendEmailToContactListCommand, Result>
{
    private readonly ILogger<SendEmailCommandHandler> _logger;
    private readonly IRepository<Email> _genericEmailRepository;
    private readonly IRepository<Recipient> _genericRecipientRepository;
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private readonly IEmailTemplateEngine _emailTemplateEngine;
    private readonly IConfigurationRoot _configuration;

    public SendEmailToContactListCommandHandler(
        ILogger<SendEmailCommandHandler> logger,
        IRepository<Email> genericEmailRepository,
        IRepository<Recipient> genericRecipientRepository,
        IEmailSendingRepository emailSendingRepository,
        IExternalEmailServiceWrapper externalEmailServiceWrapper,
        IEmailTemplateEngine emailTemplateEngine,
        IConfigurationRoot configuration)
    {
        _logger = logger;
        _genericEmailRepository = genericEmailRepository;
        _genericRecipientRepository = genericRecipientRepository;
        _emailSendingRepository = emailSendingRepository;
        _externalEmailServiceWrapper = externalEmailServiceWrapper;
        _emailTemplateEngine = emailTemplateEngine;
        _configuration = configuration;
    }
    
    public async Task<Result> Handle(SendEmailToContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Remove fake email from constructor
            request.Email.Recipients.Clear();
            
            // If in test or dev environment, only allow sending emails to whitelisted addresses
            EmailSendingUtil.ThrowExceptionIfRecipientsAreNotWhitelistedInNonProdEnv(_configuration, request.ContactList.Contacts);
            
            // Check if email can be sent or if service limits have been exhausted
            Result isRequestWithinServiceLimits = await EmailSendingUtil.ValidateRequestIsWithinServiceLimits(
                request.ContactList.Contacts.Count,
                _emailSendingRepository,
                _logger,
                _externalEmailServiceWrapper,
                _emailTemplateEngine,
                cancellationToken);
                
            if (isRequestWithinServiceLimits.IsFailed) return isRequestWithinServiceLimits;
            
            // Create batches
            int batchSize = request.BatchSize ?? ServiceLimits.RecipientsPerEmail;
            var batchCount = (int)Math.Ceiling((double)request.ContactList.Contacts.Count / batchSize);
            List<Email> emails = [];
            
            for (var i = 0; i < batchCount; i++)
            {
                var email = new Email
                {
                    Attachments = request.Email.Attachments,
                    HtmlBody = request.Email.HtmlBody,
                    PlainTextBody = request.Email.PlainTextBody,
                    Subject = request.Email.Subject,
                    SenderEmailAddress = request.Email.SenderEmailAddress,
                    Recipients = request.ContactList.Contacts.Skip(i * batchSize).Take(batchSize).ToList()
                };
                
                emails.Add(email);
            }
            
            // Add email(s) to database
            foreach (Email email in emails)
            {
                await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _genericRecipientRepository, cancellationToken);
                Result<Email> addResult = await _genericEmailRepository.AddAsync(email, cancellationToken);
                if (addResult.IsFailed)
                {
                    Result failedResult = Result.Fail(addResult.Errors);
                    return failedResult;
                }
            }
            
            // Send batches
            foreach (Email email in emails)
            {
                Result<EmailSendingStatus> sendResult = await _externalEmailServiceWrapper.SendAsync(email, request.UseBcc, cancellationToken);
                if (sendResult.IsFailed)
                {
                    Result failedResult = Result.Fail(sendResult.Errors);
                    return failedResult;
                }
            }
            
            return Result.Ok();
        } 
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{code}] - {message}", errorCode, ex.Message);
            return Result.Fail(ErrorMessages.EmailSending_Exception + errorCode);
        }
    }
}
