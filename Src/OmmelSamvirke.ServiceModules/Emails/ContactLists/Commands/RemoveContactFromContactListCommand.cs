using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Emails.Validators;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

public record RemoveContactFromContactListCommand(
    ContactList ContactList,
    string EmailAddress,
    bool IsUserAdmin) : IRequest<Result<ContactList>>;

[UsedImplicitly]
public class RemoveContactFromContactListCommandValidator : AbstractValidator<RemoveContactFromContactListCommand>
{
    public RemoveContactFromContactListCommandValidator(IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);
        RuleFor(x => x.EmailAddress)
            .Must(RecipientValidator.IsEmailStructureValid)
            .WithMessage(DomainModules.Errors.ErrorMessages.Recipient_EmailAddress_MustBeValid);
    }
}

public class RemoveContactFromContactListCommandHandler : IRequestHandler<RemoveContactFromContactListCommand, Result<ContactList>>
{
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<Recipient> _contactRepository;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public RemoveContactFromContactListCommandHandler(
        IRepository<ContactList> contactListRepository,
        IRepository<Recipient> contactRepository,
        IMediator mediator,
        ILogger logger)
    {
        _contactListRepository = contactListRepository;
        _contactRepository = contactRepository;
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<Result<ContactList>> Handle(RemoveContactFromContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if contact is in contact list
            bool isContactInContactList = request.ContactList.Contacts.Any(x => x.EmailAddress == request.EmailAddress);
            if (!isContactInContactList) return Result.Fail(ErrorMessages.ContactDoesNotExistInContactList);

            // Attempt to delete the contact
            request.ContactList.Contacts.RemoveAll(x => x.EmailAddress == request.EmailAddress);
            Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(request.ContactList, cancellationToken);
            
            if (!updateResult.IsSuccess) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
            
            // Send email notifying the user that they have been unsubscribed from the contact list
            if (!request.IsUserAdmin)
            {
                var recipient = new Recipient
                {
                    EmailAddress = request.EmailAddress,
                };
                
                await _mediator.Send(new SendEmailCommand(new Email
                {
                    SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                    Recipients = [recipient],
                    Attachments = [],
                    Subject = "", // TODO - Populate from some kind of template
                    Body = "" // TODO - Populate from some kind of template
                }), cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{code}] - {message}", errorCode, ex.Message);
            return Result.Fail(ErrorMessages.GenericErrorWithErrorCode + errorCode);
        }
    }
}
