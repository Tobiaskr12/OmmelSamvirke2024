using FluentResults;
using MediatR;
using DomainModules.Emails.Entities;

// ReSharper disable once CheckNamespace
namespace Contracts.Emails.ContactLists.Queries;

public record CountContactsInContactListQuery(int ContactListId) : IRequest<Result<int>>;

public record GetContactListQuery(int Id) : IRequest<Result<ContactList>>;

public record SearchContactListsByEmailQuery(string EmailAddress) : IRequest<Result<List<ContactList>>>;
