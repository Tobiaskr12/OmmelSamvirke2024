using Contracts.DataAccess.Emails.Enums;
using FluentResults;
using MediatR;
using DomainModules.Emails.Entities;

// ReSharper disable once CheckNamespace
namespace Contracts.Emails.Sending.Queries;

public record RecipientsValidationQuery(List<Recipient> Recipients) : IRequest<Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)>>;

public record ServiceLimitsQuery(ServiceLimitInterval Interval) : IRequest<Result<int>>;
