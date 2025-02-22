using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters.Sending;

public record SendNewsletterCommand(List<NewsletterGroup> NewsletterGroups, Email Email) : IRequest<Result>;
