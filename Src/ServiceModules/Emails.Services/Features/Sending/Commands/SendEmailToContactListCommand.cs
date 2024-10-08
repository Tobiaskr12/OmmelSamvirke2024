using Emails.Domain.Entities;
using FluentResults;
using MediatR;

namespace Emails.Services.Features.Sending.Commands;

public class SendEmailToContactListCommand : IRequest<Result<EmailSendingStatus>>
{
    public required Email Email { get; set; }
    public required List<int> ContactListIds { get; set; }
}

public class SendEmailToContactListCommandHandler : IRequestHandler<SendEmailToContactListCommand, Result<EmailSendingStatus>>
{
    public Task<Result<EmailSendingStatus>> Handle(SendEmailToContactListCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
