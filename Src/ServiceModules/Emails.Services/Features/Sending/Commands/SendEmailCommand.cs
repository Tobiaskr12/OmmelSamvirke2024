using Emails.Domain.Entities;
using Emails.Services.Errors;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;

namespace Emails.Services.Features.Sending.Commands;

public class SendEmailCommand : IRequest<Result<EmailSendingStatus>>
{
    public required Email Email { get; init; }
}

[UsedImplicitly]
public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator(IValidator<Email> emailValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, Result<EmailSendingStatus>>
{
    public async Task<Result<EmailSendingStatus>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        IEnumerable<string> errorMessage =
        [
            ErrorMessages.RequestBonked,
            ErrorMessages.RequestFailed
        ];
        
        // Result<EmailSendingStatus> result = Result.Fail(errorMessage);
        
        Result<EmailSendingStatus> result = Result.Ok(new EmailSendingStatus
        {
            Email = request.Email,
            Status = SendingStatus.SucceededValue,
            InvalidRecipients = []
        });
        
        return result;
    }
}
