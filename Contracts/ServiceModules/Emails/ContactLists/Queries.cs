using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Emails.ContactLists;

public record CountContactsInContactListQuery(int ContactListId) : IRequest<Result<int>>;

public record GetContactListQuery(int Id) : IRequest<Result<ContactList>>;

public record SearchContactListsByEmailQuery(string EmailAddress) : IRequest<Result<List<ContactList>>>;
