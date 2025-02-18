using Contracts.DataAccess.Emails.Enums;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Emails.Sending;

public record RecipientsValidationQuery(List<Recipient> Recipients) : IRequest<Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)>>;

public record ServiceLimitsQuery(ServiceLimitInterval Interval) : IRequest<Result<int>>;
