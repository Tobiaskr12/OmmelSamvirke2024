using Azure.Communication.Email;

namespace EmailWrapper.Models;

public class EmailSendingStatus
{
    public required Email Email { get; set; }
    public required List<Recipient> InvalidRecipients { get; set; }
    public required EmailSendResult Result { get; set; }
    public required EmailSendStatus Status { get; set; }
}
