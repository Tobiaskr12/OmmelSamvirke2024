using Contracts.DataAccess.Base;
using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;

public record GetContactListQuery(int Id) : IRequest<Result<ContactList>>;

public class GetContactListQueryHandler : IRequestHandler<GetContactListQuery, Result<ContactList>>
{
    private readonly IRepository<ContactList> _contactListRepository;

    public GetContactListQueryHandler(IRepository<ContactList> contactListRepository)
    {
        _contactListRepository = contactListRepository;
    }
    
    public async Task<Result<ContactList>> Handle(GetContactListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Result<ContactList> result = await _contactListRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
            
            return result.IsSuccess ?
                Result.Ok(result.Value) : 
                Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        catch (Exception)
        {
            var errorCode = Guid.NewGuid();
            return Result.Fail(ErrorMessages.GenericErrorWithErrorCode + errorCode);
        }
    }
}
