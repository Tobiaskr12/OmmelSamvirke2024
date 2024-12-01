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
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Features.Sending.Commands;

public record SendEmailCommand(Email Email) : IRequest<Result<EmailSendingStatus>>;

[UsedImplicitly]
public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator(IValidator<Email> emailValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}

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
            Result isRequestWithinServiceLimits = await ValidateRequestIsWithinServiceLimits(request, cancellationToken);
            if (isRequestWithinServiceLimits.IsFailed) return isRequestWithinServiceLimits;
            
            // Add email to database
            Result<Email> addResult = await _genericEmailRepository.AddAsync(request.Email, cancellationToken);
            if (addResult.IsFailed)
            {
                Result failedResult = Result.Fail(addResult.Errors);
                failedResult.Errors.AddRange(addResult.Errors);
                return failedResult;
            }

            // Send email
            return await _externalEmailServiceWrapper.SendAsync(request.Email, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{code}] - {message}", errorCode, ex.Message);
            return Result.Fail(ErrorMessages.EmailSending_Exception + errorCode);
        }
    }

    private async Task<Result> ValidateRequestIsWithinServiceLimits(SendEmailCommand request, CancellationToken cancellationToken)
    {
        Result<double> minuteLimitResult = await _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(
            ServiceLimitInterval.PerMinute,
            numberOfEmailsToSend: 1,
            cancellationToken);
        Result<double> hourlyLimitResult = await _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(
            ServiceLimitInterval.PerHour,
            numberOfEmailsToSend: 1,
            cancellationToken);

        if (minuteLimitResult.IsFailed || hourlyLimitResult.IsFailed)
        {
            _logger.LogError(
                "Tried sending an email, but could not check service limit usage, so the operation was aborted. Errors: {errors}",
                minuteLimitResult.Errors.Select(e => e.Message).Concat(hourlyLimitResult.Errors.Select(e => e.Message))
            );
            {
                return Result.Fail(ErrorMessages.EmailSending_ServiceLimitError);
            }
        }

        // Early return
        if (minuteLimitResult.Value < 100.00 && hourlyLimitResult.Value < 100.00) return Result.Ok();
        
        _logger.LogError(
            "Tried sending an email, but doing so would exceed at least one service limit." +
            "\nEmail: {email}" +
            "\nMinute-limit used: {minuteLimit}" +
            "\nHour-limit used: {hourLimit}",
            request.Email,
            minuteLimitResult.Value.ToString("0.00") + "%",
            hourlyLimitResult.Value.ToString("0.00") + "%");

        return Result.Fail(ErrorMessages.EmailSending_ServiceLimitError);
    }
}
