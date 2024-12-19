using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;

public record GetContactListQuery(int Id) : IRequest<Result<ContactList>>;

public class GetContactListQueryHandler : IRequestHandler<GetContactListQuery, Result<ContactList>>
{
    private readonly ILogger _logger;
    private readonly IRepository<ContactList> _contactListRepository;

    public GetContactListQueryHandler(ILogger logger, IRepository<ContactList> contactListRepository)
    {
        _logger = logger;
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
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{code}] - {message}", errorCode, ex.Message);
            return Result.Fail(ErrorMessages.GenericErrorWithErrorCode + errorCode);
        }
    }
}
