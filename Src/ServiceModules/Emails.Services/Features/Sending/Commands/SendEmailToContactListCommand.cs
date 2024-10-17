using Emails.Domain.Entities;
using Emails.DTOs;
using FluentResults;
using MediatR;

namespace Emails.Services.Features.Sending.Commands;

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
