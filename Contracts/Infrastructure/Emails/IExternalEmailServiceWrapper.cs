﻿using Contracts.ServiceModules.Emails.DTOs;
using FluentResults;
using DomainModules.Emails.Entities;

namespace Contracts.Infrastructure.Emails;

public interface IExternalEmailServiceWrapper
{
    Task<Result<EmailSendingStatus>> SendAsync(Email email, bool useBcc = false, CancellationToken cancellationToken = default);
    Task<Result> SendBatchesAsync(Email email, int batchSize, bool useBcc = false, CancellationToken cancellationToken = default);
}
