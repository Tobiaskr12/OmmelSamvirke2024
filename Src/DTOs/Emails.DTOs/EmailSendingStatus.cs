using Emails.Domain.Entities;

namespace Emails.DTOs;

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
    Canceled 
}
