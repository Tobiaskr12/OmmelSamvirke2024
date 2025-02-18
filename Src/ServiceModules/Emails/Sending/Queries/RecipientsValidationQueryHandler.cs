using Contracts.ServiceModules.Emails.Sending;
using FluentResults;
using FluentValidation.Results;
using MediatR;
using DomainModules.Emails.Entities;
using DomainModules.Emails.Validators;

namespace ServiceModules.Emails.Sending.Queries;

public class RecipientsValidationQueryHandler : 
    IRequestHandler<RecipientsValidationQuery, Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)>>
{
    public Task<Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)>> Handle(RecipientsValidationQuery request, CancellationToken cancellationToken)
    {
        (List<Recipient> valid, List<Recipient> invalid) result = (valid: [], invalid: []);
        RecipientValidator recipientValidator = new();

        foreach (Recipient recipient in request.Recipients)
        {
            ValidationResult validationResult = recipientValidator.Validate(recipient);
            if (validationResult.IsValid)
            {
                result.valid.Add(recipient);
                continue;
            }
                
            result.invalid.Add(recipient);
        }

        return Task.FromResult(Result.Ok(result));
    }
}
