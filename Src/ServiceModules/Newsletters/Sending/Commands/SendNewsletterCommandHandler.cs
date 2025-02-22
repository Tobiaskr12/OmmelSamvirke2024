using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Newsletters.Sending;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Newsletters.Sending.Commands;

public class SendNewsletterCommandHandler : IRequestHandler<SendNewsletterCommand, Result>
{
    private readonly IExternalEmailServiceWrapper _emailServiceWrapper;
        
    public SendNewsletterCommandHandler(IExternalEmailServiceWrapper emailServiceWrapper)
    {
        _emailServiceWrapper = emailServiceWrapper;
    }
        
    public async Task<Result> Handle(SendNewsletterCommand request, CancellationToken cancellationToken)
    {
        // Collect recipients from all newsletter groups' contact lists.
        var groupRecipients = new List<Recipient>();
        foreach (NewsletterGroup group in request.NewsletterGroups)
        {
            groupRecipients.AddRange(group.ContactList.Contacts);
        }
            
        // Deduplicate recipients
        List<Recipient> uniqueRecipients = groupRecipients.DistinctBy(r => r.EmailAddress).ToList();
        Email email = request.Email;
        email.Recipients = uniqueRecipients;
        
        // Send the email
        return await _emailServiceWrapper.SendBatchesAsync(
            email,
            batchSize: ServiceLimits.RecipientsPerEmail,
            useBcc: true, 
            cancellationToken
        );
    }
}
