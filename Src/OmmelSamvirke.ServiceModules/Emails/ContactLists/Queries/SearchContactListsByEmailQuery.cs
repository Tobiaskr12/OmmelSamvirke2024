using Contracts.DataAccess.Base;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;

public record SearchContactListsByEmailQuery(string EmailAddress) : IRequest<Result<List<ContactList>>>;

[UsedImplicitly]
public class SearchContactListsByEmailQueryValidator : AbstractValidator<SearchContactListsByEmailQuery>
{
    public SearchContactListsByEmailQueryValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithMessage(ErrorMessages.SearchContactListsByEmail_InvalidEmail)
            .EmailAddress()
            .WithMessage(ErrorMessages.SearchContactListsByEmail_InvalidEmail);
    }
}

public class SearchContactListsByEmailQueryHandler : IRequestHandler<SearchContactListsByEmailQuery, Result<List<ContactList>>>
{
    private readonly IRepository<ContactList> _contactListRepository;

    public SearchContactListsByEmailQueryHandler(IRepository<ContactList> contactListRepository)
    {
        _contactListRepository = contactListRepository;
    }

    public async Task<Result<List<ContactList>>> Handle(SearchContactListsByEmailQuery request, CancellationToken cancellationToken)
    {
        Result<List<ContactList>> queryResult = await _contactListRepository
            .FindAsync(
                x => x.Contacts.Any(c => c.EmailAddress == request.EmailAddress),
                cancellationToken: cancellationToken
            );
            
        return  queryResult.IsFailed ? 
            Result.Fail<List<ContactList>>(ErrorMessages.GenericErrorWithRetryPrompt) : 
            Result.Ok(queryResult.Value);
    }
}
