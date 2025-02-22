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
public class CreateContactListCommandValidator : AbstractValidator<CreateContactListCommand>
{
    public CreateContactListCommandValidator(IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);
    }
}

public class CreateContactListCommandHandler : IRequestHandler<CreateContactListCommand, Result<ContactList>>
{
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<Recipient> _recipientRepository;

    public CreateContactListCommandHandler(
        IRepository<ContactList> contactListRepository, 
        IRepository<Recipient> recipientRepository)
    {
        _contactListRepository = contactListRepository;
        _recipientRepository = recipientRepository;
    }

    public async Task<Result<ContactList>> Handle(CreateContactListCommand request, CancellationToken cancellationToken)
    {
        // Create a hash set of email addresses for efficient lookup during duplicate detection
        HashSet<string> emailSet = request.ContactList.Contacts
                                            .Select(contact => contact.EmailAddress)
                                            .ToHashSet();
            
        Result<List<Recipient>> duplicateRecipientsQuery = await _recipientRepository.FindAsync(
            x => emailSet.Contains(x.EmailAddress),
            readOnly: false,
            cancellationToken: cancellationToken
        );
            
        if (duplicateRecipientsQuery.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

        // Mapping email addresses to their index in the contacts list.
        Dictionary<string, int> indexedContacts = request.ContactList.Contacts
                                                        .Select((contact, index) => new { contact, index })
                                                        .ToDictionary(x => x.contact.EmailAddress, x => x.index);
            
        foreach (Recipient duplicate in duplicateRecipientsQuery.Value)
        {
            if (indexedContacts.TryGetValue(duplicate.EmailAddress, out int index))
            {
                request.ContactList.Contacts[index] = duplicate;
            }
        }
            
        return await _contactListRepository.AddAsync(request.ContactList, cancellationToken);
    }
}
