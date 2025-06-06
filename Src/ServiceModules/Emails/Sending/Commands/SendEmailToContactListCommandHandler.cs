using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.SupportModules.Logging;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.Sending.Commands;

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
    private readonly ILoggingHandler _logger;
    private readonly IRepository<Email> _genericEmailRepository;
    private readonly IRepository<Recipient> _genericRecipientRepository;
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private readonly IEmailTemplateEngine _emailTemplateEngine;
    private readonly IConfigurationRoot _configuration;

    public SendEmailToContactListCommandHandler(
        ILoggingHandler logger,
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
        // Remove fake email from command declaration
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
        int batchCount = (int)Math.Ceiling((double)request.ContactList.Contacts.Count / batchSize);
        List<Email> emails = [];
            
        for (int i = 0; i < batchCount; i++)
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
}
