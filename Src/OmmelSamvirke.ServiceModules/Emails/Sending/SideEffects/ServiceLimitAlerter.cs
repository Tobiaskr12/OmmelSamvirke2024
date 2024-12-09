using System.ComponentModel;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.Infrastructure.Emails;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.SideEffects;

public static class ServiceLimitAlerter
{
    public static async Task AlertIfServiceUsageIsAboveThreshold(
        ServiceLimitInterval interval,
        IExternalEmailServiceWrapper externalEmailServiceWrapper,
        double threshold,
        double currentUsage,
        ILogger logger,
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
            var warningMessage =
                $"The service limit for email sending is close to being reached for the '{Enum.GetName(typeof(ServiceLimitInterval), interval)}'-interval. Current usage is {currentUsage:0.00}%";
            
            logger.LogWarning("{}", warningMessage);
            await externalEmailServiceWrapper.SendAsync(new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Subject = "Email Service Limit Warning - OmmelSamvirke",
                Body = warningMessage,
                Attachments = [],
                Recipients = [new Recipient { EmailAddress = "tobiaskristensen12@gmail.com" }]
            }, cancellationToken);
        }
    }
}
