using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters.Subscriptions;

public class SubscribeToNewslettersCommand : IRequest<Result>
{
    public string EmailAddress { get; }
    public List<int> NewsletterGroupIds { get; }

    public SubscribeToNewslettersCommand(string emailAddress, List<int> newsletterGroupIds)
    {
        EmailAddress = emailAddress;
        NewsletterGroupIds = newsletterGroupIds;
    }
}
