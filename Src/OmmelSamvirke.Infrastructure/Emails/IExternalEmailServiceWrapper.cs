using FluentResults;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;

namespace OmmelSamvirke.Infrastructure.Emails;

public interface IExternalEmailServiceWrapper
{
    Task<Result<EmailSendingStatus>> SendAsync(Email email, CancellationToken cancellationToken = default);
}
