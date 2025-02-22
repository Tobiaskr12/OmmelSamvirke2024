using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Contracts.DataAccess.Emails.Enums;
using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.Extensions.Configuration;
using DomainModules.Emails.Entities;
using ServiceModules.Emails.Sending.SideEffects;
using ServiceModules.Errors;

namespace ServiceModules.Emails.Sending;

public static class EmailSendingUtil
{
    public static async Task<Result> ValidateRequestIsWithinServiceLimits(
        int numberOfEmailsToSend,
        IEmailSendingRepository emailSendingRepository,
        ILoggingHandler logger,
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
            IEnumerable<string> errors = 
                minuteLimitResult.Errors
                    .Select(e => e.Message)
                    .Concat(hourlyLimitResult.Errors.Select(e => e.Message));
            
            logger.LogError(null, 
                $"Tried sending an email, but could not check service limit usage, so the operation was aborted. Errors: {errors}"
            );
            
            return Result.Fail(ErrorMessages.EmailSending_ServiceLimitError);
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
        logger.LogError(new Exception(
            $"Tried sending one or more emails, but doing so would exceed at least one service limit." +
            $"\nEmails to send: {numberOfEmailsToSend}" +
            $"\nMinute-limit used: {minuteLimitResult.Value.ToString("0.00") + "%"}" +
            $"\nHour-limit used: {hourlyLimitResult.Value.ToString("0.00") + "%"}")
        );

        return Result.Fail(ErrorMessages.EmailSending_ServiceLimitError);
    }

    /// <summary>
    /// Find existing recipients in the database and replace the provided recipients with the found entities
    /// to avoid saving duplicated recipients
    /// </summary>
    public static async Task<Result> FetchAndReplaceExistingRecipients(
        Email email,
        IRepository<Recipient> recipientRepository,
        CancellationToken cancellationToken)
    {
        // If there are no recipients, nothing to do.
        if (email.Recipients.Count == 0)
        {
            return Result.Ok();
        }

        // Normalize email addresses from the email's recipients.
        var normalizedEmails = email.Recipients
                                    .Select(r => r.EmailAddress.Trim().ToUpperInvariant())
                                    .Distinct()
                                    .ToList();

        // Fetch all recipients from the repository whose normalized email is in our list.
        Result<List<Recipient>> findResult = await recipientRepository.FindAsync(
            r => normalizedEmails.Contains(r.EmailAddress),
            readOnly: false,
            cancellationToken: cancellationToken);

        if (findResult.IsFailed)
        {
            return Result.Fail(findResult.Errors);
        }

        // Build a lookup dictionary from the fetched recipients.
        var lookup = findResult.Value
                               .ToDictionary(
                                   r => r.EmailAddress.Trim().ToUpperInvariant(),
                                   r => r,
                                   StringComparer.OrdinalIgnoreCase);

        // Replace each recipient in the email with the one from the lookup, if it exists.
        for (int i = 0; i < email.Recipients.Count; i++)
        {
            string norm = email.Recipients[i].EmailAddress.Trim().ToUpperInvariant();
            if (lookup.TryGetValue(norm, out Recipient existing))
            {
                email.Recipients[i] = existing;
            }
        }

        // Deduplicate the recipients (in case the same email appeared more than once).
        email.Recipients = email.Recipients
                                .GroupBy(r => r.EmailAddress.Trim().ToUpperInvariant())
                                .Select(g => g.First())
                                .ToList();

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
