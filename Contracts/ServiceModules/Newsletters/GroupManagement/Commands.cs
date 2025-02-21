using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters.GroupManagement;

public record CreateNewsletterGroupCommand(NewsletterGroup NewsletterGroup) : IRequest<Result<NewsletterGroup>>;
public record UpdateNewsletterGroupCommand(NewsletterGroup NewsletterGroup) : IRequest<Result<NewsletterGroup>>;
public record DeleteNewsletterGroupCommand(int NewsletterGroupId) : IRequest<Result>;
