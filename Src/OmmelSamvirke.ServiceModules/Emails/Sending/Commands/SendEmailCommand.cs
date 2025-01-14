using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

public record SendEmailCommand(Email Email) : IRequest<Result<EmailSendingStatus>>;

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
    private readonly ILogger<SendEmailCommandHandler> _logger;
    private readonly IRepository<Email> _genericEmailRepository;
    private readonly IRepository<Recipient> _genericRecipientRepository;
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly IConfigurationRoot _configuration;
    private readonly IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private readonly IEmailTemplateEngine _emailTemplateEngine;

    public SendEmailCommandHandler(
        ILogger<SendEmailCommandHandler> logger,
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
        try
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
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{code}] - {message}", errorCode, ex.Message);
            return Result.Fail(ErrorMessages.EmailSending_Exception + errorCode);
        }
    }
}
