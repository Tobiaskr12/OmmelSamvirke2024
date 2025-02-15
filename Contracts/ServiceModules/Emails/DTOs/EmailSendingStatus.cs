using Contracts.ServiceModules.Emails.Enums;
using DomainModules.Emails.Entities;

namespace Contracts.ServiceModules.Emails.DTOs;

public record EmailSendingStatus
(
    Email Email,
    SendingStatus Status,
    List<Recipient> InvalidRecipients
);
