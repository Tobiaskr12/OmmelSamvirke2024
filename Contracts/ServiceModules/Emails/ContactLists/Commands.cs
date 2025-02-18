using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Emails.ContactLists;

public record AddContactToContactListCommand(ContactList ContactList, Recipient Contact) : IRequest<Result<ContactList>>;

public record CreateContactListCommand(ContactList ContactList) : IRequest<Result<ContactList>>;

public record RemoveContactFromContactListCommand(ContactList ContactList, string EmailAddress, bool IsUserAdmin) : IRequest<Result<ContactList>>;

public record UndoUnsubscribeFromContactListCommand(string EmailAddress, Guid UnsubscribeToken, Guid UndoToken) : IRequest<Result>;

public record UnsubscribeFromContactListCommand(string EmailAddress, Guid UnsubscribeToken) : IRequest<Result>;
