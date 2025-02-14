using FluentResults;
using FluentValidation.Results;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Emails.Validators;

namespace OmmelSamvirke.ServiceModules.Emails.Sending.Queries;

public record RecipientsValidationQuery(List<Recipient> Recipients) 
    : IRequest<Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)>>;

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
