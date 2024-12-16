using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.ServiceModules.Emails.Sending.SideEffects;
using OmmelSamvirke.ServiceModules.Emails.Util;
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
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly IExternalEmailServiceWrapper _externalEmailServiceWrapper;

    public SendEmailCommandHandler(
        ILogger<SendEmailCommandHandler> logger,
        IRepository<Email> genericEmailRepository,
        IEmailSendingRepository emailSendingRepository,
        IExternalEmailServiceWrapper externalEmailServiceWrapper)
    {
        _logger = logger;
        _genericEmailRepository = genericEmailRepository;
        _emailSendingRepository = emailSendingRepository;
        _externalEmailServiceWrapper = externalEmailServiceWrapper;
    }
    
    public async Task<Result<EmailSendingStatus>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email can be sent or if service limits have been exhausted
            Result isRequestWithinServiceLimits = await ServiceLimitValidator.ValidateRequestIsWithinServiceLimits(
                1,
                _emailSendingRepository,
                _logger,
                _externalEmailServiceWrapper,
                cancellationToken);
            
            if (isRequestWithinServiceLimits.IsFailed) return isRequestWithinServiceLimits;
            
            // Add email to database
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
