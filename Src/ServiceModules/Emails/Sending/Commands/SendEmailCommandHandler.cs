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
using DomainModules.Emails.Entities;

namespace ServiceModules.Emails.Sending.Commands;

[UsedImplicitly]
public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator(IValidator<Email> emailValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}

/// <summary>
/// This commands sends an <see cref="Email"/>.
/// The email is only sent if sending the email doesn't exceed the service limits.
/// </summary>
public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, Result<EmailSendingStatus>>
{
    private readonly ILoggingHandler _logger;
    private readonly IRepository<Email> _genericEmailRepository;
    private readonly IRepository<Recipient> _genericRecipientRepository;
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly IConfigurationRoot _configuration;
    private readonly IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private readonly IEmailTemplateEngine _emailTemplateEngine;

    public SendEmailCommandHandler(
        ILoggingHandler logger,
        IRepository<Email> genericEmailRepository,
        IRepository<Recipient> genericRecipientRepository,
        IEmailSendingRepository emailSendingRepository,
        IConfigurationRoot configuration,
        IExternalEmailServiceWrapper externalEmailServiceWrapper,
        IEmailTemplateEngine emailTemplateEngine)
    {
        _logger = logger;
        _genericEmailRepository = genericEmailRepository;
        _genericRecipientRepository = genericRecipientRepository;
        _emailSendingRepository = emailSendingRepository;
        _configuration = configuration;
        _externalEmailServiceWrapper = externalEmailServiceWrapper;
        _emailTemplateEngine = emailTemplateEngine;
    }
    
    public async Task<Result<EmailSendingStatus>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        // If in test or dev environment, only allow sending emails to whitelisted addresses
        EmailSendingUtil.ThrowExceptionIfRecipientsAreNotWhitelistedInNonProdEnv(_configuration, request.Email.Recipients);
            
        // Check if email can be sent or if service limits have been exhausted
        Result isRequestWithinServiceLimits = await EmailSendingUtil.ValidateRequestIsWithinServiceLimits(
            1,
            _emailSendingRepository,
            _logger,
            _externalEmailServiceWrapper,
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

        // Send email
        return await _externalEmailServiceWrapper.SendAsync(request.Email, cancellationToken: cancellationToken);
    }
}
