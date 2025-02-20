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

public class ConfirmNewsletterSubscriptionCommand : IRequest<Result>
{
    public Guid Token { get; }

    public ConfirmNewsletterSubscriptionCommand(Guid token)
    {
        Token = token;
    }
}
