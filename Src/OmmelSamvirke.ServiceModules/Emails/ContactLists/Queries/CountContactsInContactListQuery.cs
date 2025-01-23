using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;

public record CountContactsInContactListQuery(int ContactListId) : IRequest<Result<int>>;

[UsedImplicitly]
public class GetContactListContactCountQueryValidator : AbstractValidator<CountContactsInContactListQuery>
{
    public GetContactListContactCountQueryValidator()
    {
        RuleFor(x => x.ContactListId)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}

public class CountContactsInContactListQueryHandler : IRequestHandler<CountContactsInContactListQuery, Result<int>>
{
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly ILogger _logger;

    public CountContactsInContactListQueryHandler(
        IRepository<ContactList> contactListRepository,
        ILogger logger)
    {
        _contactListRepository = contactListRepository;
        _logger = logger;
    }
    
    public async Task<Result<int>> Handle(CountContactsInContactListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Result<ContactList> result = await _contactListRepository.GetByIdAsync(request.ContactListId, cancellationToken: cancellationToken);

            if (result is { IsSuccess: true, Value: not null })
            {
                int contactCount = result.Value.Contacts.Count;
                return Result.Ok(contactCount);
            }

            return Result.Fail<int>(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{Code}] - {Message}", errorCode, ex.Message);
            return Result.Fail<int>(ErrorMessages.GenericErrorWithErrorCode + errorCode);
        }
    }
}
