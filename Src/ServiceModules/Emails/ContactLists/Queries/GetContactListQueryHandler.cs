using Contracts.DataAccess.Base;
using Contracts.Emails.ContactLists.Queries;
using FluentResults;
using MediatR;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.ContactLists.Queries;

public class GetContactListQueryHandler : IRequestHandler<GetContactListQuery, Result<ContactList>>
{
    private readonly IRepository<ContactList> _contactListRepository;

    public GetContactListQueryHandler(IRepository<ContactList> contactListRepository)
    {
        _contactListRepository = contactListRepository;
    }
    
    public async Task<Result<ContactList>> Handle(GetContactListQuery request, CancellationToken cancellationToken)
    {
        Result<ContactList> result = await _contactListRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
            
        return result.IsSuccess ?
            Result.Ok(result.Value) : 
            Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}
