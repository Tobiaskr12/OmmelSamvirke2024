using System.ComponentModel;
using FluentResults;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.Interfaces.Emails;
using OmmelSamvirke.SupportModules.Logging.Interfaces;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.SideEffects;

public static class ServiceLimitAlerter
{
    public static async Task AlertIfServiceUsageIsAboveThreshold(
        ServiceLimitInterval interval,
        IExternalEmailServiceWrapper externalEmailServiceWrapper,
        double threshold,
        double currentUsage,
        ILoggingHandler logger,
        IEmailTemplateEngine emailTemplateEngine,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(ServiceLimitInterval), interval)) 
            throw new InvalidEnumArgumentException(
                nameof(interval),
                (int)interval,
                typeof(ServiceLimitInterval)
            );
        
        if (currentUsage >= threshold)
        {
            // var warningMessage =
            //     $"The service limit for email sending is close to being reached for the '{Enum.GetName(typeof(ServiceLimitInterval), interval)}'-interval. Current usage is {currentUsage:0.00}%";
            Result result = emailTemplateEngine.GenerateBodiesFromTemplate("Empty.html"); // TODO - Fix warning message to use HTML template
            if (result.IsFailed) throw new Exception("Email body generation failed");
            
            logger.LogWarning($"{emailTemplateEngine.GetPlainTextBody()}");
            await externalEmailServiceWrapper.SendAsync(new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Subject = "Email Service Limit Warning - OmmelSamvirke",
                HtmlBody = emailTemplateEngine.GetHtmlBody(),
                PlainTextBody = emailTemplateEngine.GetPlainTextBody(),
                Attachments = [],
                Recipients = [new Recipient { EmailAddress = "tobiaskristensen12@gmail.com" }]
            }, cancellationToken: cancellationToken);
        }
    }
}
