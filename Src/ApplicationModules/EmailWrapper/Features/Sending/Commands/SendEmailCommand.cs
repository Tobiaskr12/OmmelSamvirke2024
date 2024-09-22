using EmailWrapper.Models;
using FluentResults;
using MediatR;

namespace EmailWrapper.Features.Sending.Commands;

public class SendEmailCommand : IRequest<Result<EmailSendingStatus>>
{
    public required Email Email { get; set; }
}

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, Result<EmailSendingStatus>>
{
    public Task<Result<EmailSendingStatus>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
