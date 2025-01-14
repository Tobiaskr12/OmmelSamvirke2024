using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.ServiceModules.Emails.Sending.SideEffects;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.Sending;

public static class EmailSendingUtil
{
    public static async Task<Result> ValidateRequestIsWithinServiceLimits(
        int numberOfEmailsToSend,
        IEmailSendingRepository emailSendingRepository,
        ILogger logger,
        IExternalEmailServiceWrapper externalEmailServiceWrapper,
        IEmailTemplateEngine emailTemplateEngine,
        CancellationToken cancellationToken)
    {
        // Calculate the effect of sending the email against the service limits
        Result<double> minuteLimitResult = await emailSendingRepository.CalculateServiceLimitAfterSendingEmails(
            ServiceLimitInterval.PerMinute,
            numberOfEmailsToSend,
            cancellationToken);
        Result<double> hourlyLimitResult = await emailSendingRepository.CalculateServiceLimitAfterSendingEmails(
            ServiceLimitInterval.PerHour,
            numberOfEmailsToSend,
            cancellationToken);

        // If the usage can't be calculated, log the error and return failure
        if (minuteLimitResult.IsFailed || hourlyLimitResult.IsFailed)
        {
            logger.LogError(
                "Tried sending an email, but could not check service limit usage, so the operation was aborted. Errors: {errors}",
                minuteLimitResult.Errors.Select(e => e.Message).Concat(hourlyLimitResult.Errors.Select(e => e.Message))
            );
            {
                return Result.Fail(ErrorMessages.EmailSending_ServiceLimitError);
            }
        }
        
        // Log and send high service limit usage warning to developer if service limit usage exceed threshold
        const double warningThreshold = 80.0;
        await ServiceLimitAlerter.AlertIfServiceUsageIsAboveThreshold(
            ServiceLimitInterval.PerHour,
            externalEmailServiceWrapper,
            warningThreshold,
            hourlyLimitResult.Value,
            logger,
            emailTemplateEngine,
            cancellationToken);
        await ServiceLimitAlerter.AlertIfServiceUsageIsAboveThreshold(
            ServiceLimitInterval.PerMinute,
            externalEmailServiceWrapper,
            warningThreshold,
            minuteLimitResult.Value,
            logger,
            emailTemplateEngine,
            cancellationToken);

        // Return if both values are below 100%
        if (minuteLimitResult.Value < 100.00 && hourlyLimitResult.Value < 100.00) return Result.Ok();
        
        // Else, Log error and return failure 
        logger.LogError(
            "Tried sending one or more emails, but doing so would exceed at least one service limit." +
            "\nEmails to send: {emailsToSend}" +
            "\nMinute-limit used: {minuteLimit}" +
            "\nHour-limit used: {hourLimit}",
            numberOfEmailsToSend,
            minuteLimitResult.Value.ToString("0.00") + "%",
            hourlyLimitResult.Value.ToString("0.00") + "%");

        return Result.Fail(ErrorMessages.EmailSending_ServiceLimitError);
    }

    /// <summary>
    /// Find existing recipients in the database and replace the provided recipients with the found entities
    /// to avoid saving duplicated recipients
    /// </summary>
    public static async Task<Result> FetchAndReplaceExistingRecipients(Email email, IRepository<Recipient> repository, CancellationToken cancellationToken)
    {
        List<string> recipientEmails = email.Recipients.Select(r => r.EmailAddress).ToList();
        Result<List<Recipient>> existingRecipientsResult = await repository.FindAsync(
            r => recipientEmails.Contains(r.EmailAddress),
            cancellationToken: cancellationToken);
            
        if (existingRecipientsResult.IsFailed)
        {
            return Result.Fail(existingRecipientsResult.Errors);
        }
            
        List<Recipient>? existingRecipients = existingRecipientsResult.Value;
            
        for (var i = 0; i < email.Recipients.Count; i++)
        {
            Recipient newRecipient = email.Recipients[i];
            Recipient? existingRecipient = existingRecipients.FirstOrDefault(r => r.EmailAddress == newRecipient.EmailAddress);

            if (existingRecipient is not null)
            {
                email.Recipients[i] = existingRecipient;
            }
        }
        
        return Result.Ok();
    }

    /// <summary>
    /// This method checks if the email recipients are whitelisted in non-prod environments.
    /// This is to ensure that no emails are sent to non-allowed email addresses during development and testing.
    /// If the application is not configured correctly or if a disallowed recipient is detected, an exception is thrown.
    /// </summary>
    public static void ThrowExceptionIfRecipientsAreNotWhitelistedInNonProdEnv(IConfigurationRoot configuration, List<Recipient> recipients)
    {
        IConfigurationSection executionEnvironmentSection = configuration.GetSection("ExecutionEnvironment");
        if (executionEnvironmentSection.Value is null) throw new Exception("Execution environment section is null. Refusing to send email");

        if (executionEnvironmentSection.Value != "Prod")
        {
            IConfigurationSection emailWhitelistSection = configuration.GetSection("EmailWhitelist");
            if (emailWhitelistSection.Value is null) 
                throw new Exception("Email whitelist section is null in a non-production environment. Refusing to send email");
                
            string[] emailWhitelist = emailWhitelistSection.Value.Split(";");
            if (recipients.Any(x => !emailWhitelist.Contains(x.EmailAddress)))
            {
                throw new Exception("Refusing to send the email because at least one email address is not whitelisted");
            }
        }
    }
}
