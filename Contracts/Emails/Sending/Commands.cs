using Contracts.ServiceModules.Emails.DTOs;
using FluentResults;
using MediatR;
using DomainModules.Emails.Entities;

// ReSharper disable once CheckNamespace
namespace Contracts.Emails.Sending.Commands;

public record SendEmailCommand(Email Email) : IRequest<Result<EmailSendingStatus>>;

public class SendEmailToContactListCommand : IRequest<Result>
{
    public Email Email { get; }
    public ContactList ContactList { get; }
    public int? BatchSize { get; }
    public bool UseBcc { get; }

    public SendEmailToContactListCommand(Email email, ContactList contactList, int? batchSize = null, bool useBcc = false)
    {
        Email = email;
        ContactList = contactList;
        BatchSize = batchSize;
        UseBcc = useBcc;

        // This is needed to pass validator - Will be removed in the handler
        Email.Recipients = [new Recipient { EmailAddress = "fake@example.com" }];
    }
}
