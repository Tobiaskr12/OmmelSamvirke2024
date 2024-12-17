using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

public record AddContactToContactListCommand(ContactList ContactList, Recipient Contact) : IRequest<Result<ContactList>>;

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
    private readonly ILogger _logger;

    public AddContactToContactListCommandHandler(
        IRepository<ContactList> contactListRepository,
        IRepository<Recipient> recipientRepository,
        ILogger logger)
    {
        _contactListRepository = contactListRepository;
        _recipientRepository = recipientRepository;
        _logger = logger;
    }
    
    public async Task<Result<ContactList>> Handle(AddContactToContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Result<List<Recipient>> recipientQueryResult = await _recipientRepository.FindAsync(x => 
                x.EmailAddress == request.Contact.EmailAddress, 
                cancellationToken: cancellationToken);

            if (!recipientQueryResult.IsSuccess) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);

            Recipient recipient = request.Contact;
            if (recipientQueryResult.Value is not null && recipientQueryResult.Value.Count > 0)
            {
                recipient = recipientQueryResult.Value.First();
            }
            
            request.ContactList.Contacts.Add(recipient);
            Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(request.ContactList, cancellationToken);

            return updateResult.IsSuccess ? 
                Result.Ok(updateResult.Value) : 
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
