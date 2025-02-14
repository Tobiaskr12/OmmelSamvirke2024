using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Emails.Validators;
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
    private readonly IEmailTemplateEngine _emailTemplateEngine;
    private readonly IMediator _mediator;

    public RemoveContactFromContactListCommandHandler(
        IRepository<ContactList> contactListRepository,
        IEmailTemplateEngine emailTemplateEngine,
        IMediator mediator)
    {
        _contactListRepository = contactListRepository;
        _emailTemplateEngine = emailTemplateEngine;
        _mediator = mediator;
    }
    
    public async Task<Result<ContactList>> Handle(RemoveContactFromContactListCommand request, CancellationToken cancellationToken)
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

            Result result = _emailTemplateEngine.GenerateBodiesFromTemplate("Empty.html"); // TODO - Populate from some kind of template
            if (result.IsFailed) throw new Exception("Email body generation failed.");
                
            await _mediator.Send(new SendEmailCommand(new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Recipients = [recipient],
                Attachments = [],
                Subject = "", // TODO - Populate from some kind of template
                HtmlBody = _emailTemplateEngine.GetHtmlBody(), 
                PlainTextBody = _emailTemplateEngine.GetPlainTextBody()
            }), cancellationToken);
        }

        return Result.Ok();
    }
}
