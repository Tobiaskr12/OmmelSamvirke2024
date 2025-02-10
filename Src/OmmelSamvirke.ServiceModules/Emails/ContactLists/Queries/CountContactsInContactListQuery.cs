using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;

public record CountContactsInContactListQuery(int ContactListId) : IRequest<Result<int>>;

[UsedImplicitly]
public class CountContactsInContactListQueryValidator : AbstractValidator<CountContactsInContactListQuery>
{
    public CountContactsInContactListQueryValidator()
    {
        RuleFor(x => x.ContactListId)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}

public class CountContactsInContactListQueryHandler : IRequestHandler<CountContactsInContactListQuery, Result<int>>
{
    private readonly IRepository<ContactList> _contactListRepository;

    public CountContactsInContactListQueryHandler(IRepository<ContactList> contactListRepository)
    {
        _contactListRepository = contactListRepository;
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
        catch (Exception)
        {
            var errorCode = Guid.NewGuid();
            return Result.Fail<int>(ErrorMessages.GenericErrorWithErrorCode + errorCode);
        }
    }
}
