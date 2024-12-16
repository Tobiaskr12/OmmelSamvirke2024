using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

public class SendEmailToContactListCommand : IRequest<Result<EmailSendingStatus>>
{
    public Email Email { get; }
    public ContactList ContactList { get; }

    public SendEmailToContactListCommand(Email email, ContactList contactList)
    {
        Email = email;
        ContactList = contactList;
        
        Email.Recipients = ContactList.Contacts;
    }
}

[UsedImplicitly]
public class SendEmailToContactListCommandValidator : AbstractValidator<SendEmailToContactListCommand>
{
    public SendEmailToContactListCommandValidator(IValidator<Email> emailValidator, IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);
    }
}

public class SendEmailToContactListCommandHandler : IRequestHandler<SendEmailToContactListCommand, Result<EmailSendingStatus>>
{
    public Task<Result<EmailSendingStatus>> Handle(SendEmailToContactListCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
