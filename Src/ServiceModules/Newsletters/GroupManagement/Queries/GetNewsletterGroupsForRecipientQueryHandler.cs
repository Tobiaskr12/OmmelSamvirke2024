using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.GroupManagement.Queries;

public class GetNewsletterGroupsForRecipientQueryHandler : IRequestHandler<GetNewsletterGroupsForRecipientQuery, Result<List<NewsletterGroup>>>
{
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;
    public GetNewsletterGroupsForRecipientQueryHandler(IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _newsletterGroupRepository = newsletterGroupRepository;
    }

    public async Task<Result<List<NewsletterGroup>>> Handle(GetNewsletterGroupsForRecipientQuery request, CancellationToken cancellationToken)
    {
        Result<List<NewsletterGroup>> allGroupsResult = 
            await _newsletterGroupRepository.GetAllAsync(cancellationToken: cancellationToken, readOnly: false);
        
        if (allGroupsResult.IsFailed)
            return Result.Fail<List<NewsletterGroup>>(ErrorMessages.GenericErrorWithRetryPrompt);
        
        List<NewsletterGroup> filteredGroups = 
            allGroupsResult.Value
                .Where(g => g.ContactList.Contacts.Any(r => r.EmailAddress.Equals(request.EmailAddress, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

        return Result.Ok(filteredGroups);
    }
}
