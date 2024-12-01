using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;

namespace OmmelSamvirke.ServiceModules.Emails.Features.Sending.Commands;

public record SendEmailToContactListCommand
(
    Email Email,
    List<int> ContactListIds
) : IRequest<Result<EmailSendingStatus>>;

public class SendEmailToContactListCommandHandler : IRequestHandler<SendEmailToContactListCommand, Result<EmailSendingStatus>>
{
    public Task<Result<EmailSendingStatus>> Handle(SendEmailToContactListCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
