using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DTOs.Emails;

public record EmailSendingStatus
(
    Email Email,
    SendingStatus Status,
    List<Recipient> InvalidRecipients
);

public enum SendingStatus
{
    NotStarted, 
    Running,
    Succeeded,
    Failed, 
    Canceled,
    Unknown
}
