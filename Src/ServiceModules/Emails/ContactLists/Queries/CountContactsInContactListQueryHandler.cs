using Contracts.DataAccess.Base;
using Contracts.Emails.ContactLists.Queries;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.ContactLists.Queries;

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
        Result<ContactList> result = await _contactListRepository.GetByIdAsync(request.ContactListId, cancellationToken: cancellationToken);

        if (result is { IsSuccess: true, Value: not null })
        {
            int contactCount = result.Value.Contacts.Count;
            return Result.Ok(contactCount);
        }

        return Result.Fail<int>(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}
