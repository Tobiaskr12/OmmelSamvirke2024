using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters.GroupManagement;

public record GetAllNewsletterGroupsQuery : IRequest<Result<List<NewsletterGroup>>>;
public record GetNewsletterGroupsForRecipientQuery(string EmailAddress) : IRequest<Result<List<NewsletterGroup>>>;
