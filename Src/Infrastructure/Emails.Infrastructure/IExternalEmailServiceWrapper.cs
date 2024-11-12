using Emails.Domain.Entities;
using FluentResults;

namespace Emails.Infrastructure;

public interface IExternalEmailServiceWrapper
{
    Task<Result> SendAsync(Email email, CancellationToken cancellationToken = default);
}
