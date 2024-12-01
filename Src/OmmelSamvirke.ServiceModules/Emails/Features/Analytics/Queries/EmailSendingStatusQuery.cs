using FluentResults;
using MediatR;
using OmmelSamvirke.DTOs.Emails;

namespace OmmelSamvirke.ServiceModules.Emails.Features.Analytics.Queries;

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
