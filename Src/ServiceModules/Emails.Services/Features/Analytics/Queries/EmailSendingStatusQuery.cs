using Emails.DTOs;
using FluentResults;
using MediatR;

namespace Emails.Services.Features.Analytics.Queries;

public class EmailSendingStatusQuery : IRequest<Result<EmailSendingStatus>>
{
    public required int Id { get; init; }
}

public class EmailsSendingStatusQueryHandler : IRequestHandler<EmailSendingStatusQuery, Result<EmailSendingStatus>>
{
    public Task<Result<EmailSendingStatus>> Handle(EmailSendingStatusQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
