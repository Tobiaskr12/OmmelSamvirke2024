namespace Emails.Domain.Entities;

public class EmailSendingStatus
{
    public required Email Email { get; set; }
    public required List<Recipient> InvalidRecipients { get; set; }
    public required SendingStatus Status { get; set; }
}

public enum SendingStatus
{
    NotStartedValue, 
    RunningValue,
    SucceededValue,
    FailedValue, 
    CanceledValue 
}
