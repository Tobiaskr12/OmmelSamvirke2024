using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.ContactLists.Commands;

[UsedImplicitly]
public class AddContactToContactListCommandValidator : AbstractValidator<AddContactToContactListCommand>
{
    public AddContactToContactListCommandValidator(IValidator<ContactList> contactListValidator, IValidator<Recipient> recipientValidator)
    {
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);
        RuleFor(x => x.Contact).SetValidator(recipientValidator);
    }
}

public class AddContactToContactListCommandHandler : IRequestHandler<AddContactToContactListCommand, Result<ContactList>>
{
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<Recipient> _recipientRepository;

    public AddContactToContactListCommandHandler(
        IRepository<ContactList> contactListRepository,
        IRepository<Recipient> recipientRepository)
    {
        _contactListRepository = contactListRepository;
        _recipientRepository = recipientRepository;
    }
    
    public async Task<Result<ContactList>> Handle(AddContactToContactListCommand request, CancellationToken cancellationToken)
    {
        // Attempt to find an existing recipient with the same email address.
        Result<List<Recipient>> recipientQueryResult = await _recipientRepository.FindAsync(
            x => x.EmailAddress == request.Contact.EmailAddress,
            cancellationToken: cancellationToken
        );

        if (recipientQueryResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        if (recipientQueryResult.Value?.Count > 0)
        {
            return Result.Fail(ErrorMessages.ContactList_AddContact_ContactAlreadyExitsts);
        }
            
        request.ContactList.Contacts.Add(request.Contact);
        Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(request.ContactList, cancellationToken);

        return updateResult.IsSuccess ? 
            Result.Ok(updateResult.Value) : 
            Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
    }
}
