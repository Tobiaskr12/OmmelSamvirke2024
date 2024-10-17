using Emails.Domain.Entities;

namespace Emails.DTOs;

public class EmailSendingStatus
{
    public required Email Email { get; init; }
    public required List<Recipient> InvalidRecipients { get; init; }
    public required SendingStatus Status { get; init; }
}

public enum SendingStatus
{
    NotStarted, 
    Running,
    Succeeded,
    Failed, 
    Canceled 
}
