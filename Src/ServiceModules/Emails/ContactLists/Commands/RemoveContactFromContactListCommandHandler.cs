using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.ContactLists;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using DomainModules.Common;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Emails.ContactLists.Commands;

[UsedImplicitly]
public class RemoveContactFromContactListCommandValidator : AbstractValidator<RemoveContactFromContactListCommand>
{
    public RemoveContactFromContactListCommandValidator(IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);
        RuleFor(x => x.EmailAddress)
            .Must(ValidationUtils.IsEmailStructureValid)
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

            Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(Templates.ContactLists.UserRemovedFromContactList,
                ("ContactListName", request.ContactList.Name)
            );
            if (result.IsFailed) throw new Exception("Email body generation failed.");
                
            await _mediator.Send(new SendEmailCommand(new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Recipients = [recipient],
                Attachments = [],
                Subject = _emailTemplateEngine.GetSubject(),
                HtmlBody = _emailTemplateEngine.GetHtmlBody(), 
                PlainTextBody = _emailTemplateEngine.GetPlainTextBody()
            }), cancellationToken);
        }

        return Result.Ok();
    }
}
