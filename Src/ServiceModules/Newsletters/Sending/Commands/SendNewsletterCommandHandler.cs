using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Newsletters.Sending;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Emails.Sending;

namespace ServiceModules.Newsletters.Sending.Commands;

public class SendNewsletterCommandHandler : IRequestHandler<SendNewsletterCommand, Result>
{
    private readonly IExternalEmailServiceWrapper _emailServiceWrapper;
    private readonly IRepository<Email> _genericEmailRepository;
    private readonly IRepository<Recipient> _genericRecipientRepository;
    private readonly ILoggingHandler _logger;
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly IEmailTemplateEngine _emailTemplateEngine;

    public SendNewsletterCommandHandler(
        IExternalEmailServiceWrapper emailServiceWrapper,
        IRepository<Email> genericEmailRepository,
        IRepository<Recipient> genericRecipientRepository,
        ILoggingHandler logger,
        IEmailSendingRepository emailSendingRepository,
        IEmailTemplateEngine emailTemplateEngine)
    {
        _emailServiceWrapper = emailServiceWrapper;
        _genericEmailRepository = genericEmailRepository;
        _genericRecipientRepository = genericRecipientRepository;
        _logger = logger;
        _emailSendingRepository = emailSendingRepository;
        _emailTemplateEngine = emailTemplateEngine;
    }
        
    public async Task<Result> Handle(SendNewsletterCommand request, CancellationToken cancellationToken)
    {
        // Collect recipients from all newsletter groups' contact lists.
        var groupRecipients = new List<Recipient>();
        foreach (NewsletterGroup group in request.NewsletterGroups)
        {
            groupRecipients.AddRange(group.ContactList.Contacts);
        }
            
        // Deduplicate recipients
        List<Recipient> uniqueRecipients = groupRecipients.DistinctBy(r => r.EmailAddress).ToList();
        Email email = request.Email;
        email.IsNewsletter = true;
        email.Recipients = uniqueRecipients;
        
        // Check if email can be sent or if service limits have been exhausted
        Result isRequestWithinServiceLimits = await EmailSendingUtil.ValidateRequestIsWithinServiceLimits(
            numberOfEmailsToSend: email.Recipients.Count,
            _emailSendingRepository,
            _logger,
            _emailServiceWrapper,
            _emailTemplateEngine,
            cancellationToken);
            
        if (isRequestWithinServiceLimits.IsFailed) return isRequestWithinServiceLimits;
        
        // Add email to database
        await EmailSendingUtil.FetchAndReplaceExistingRecipients(request.Email, _genericRecipientRepository, cancellationToken);
        Result<Email> addResult = await _genericEmailRepository.AddAsync(request.Email, cancellationToken);
        if (addResult.IsFailed)
        {
            Result failedResult = Result.Fail(addResult.Errors);
            return failedResult;
        }
        
        // Send the email
        return await _emailServiceWrapper.SendBatchesAsync(
            email,
            batchSize: ServiceLimits.RecipientsPerEmail,
            useBcc: true, 
            cancellationToken
        );
    }
}
