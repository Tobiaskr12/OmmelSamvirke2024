using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using FluentValidation;

namespace DomainModules.Newsletters.Validators;

public class NewsletterUnsubscribeConfirmationValidator : AbstractValidator<NewsletterUnsubscribeConfirmation>
{
    public NewsletterUnsubscribeConfirmationValidator()
    {
        RuleFor(x => x.ConfirmationToken)
            .NotNull()
            .WithMessage(ErrorMessages.NewsletterSubscriptionAction_Token_NotEmpty)
            .Must(x => x != Guid.Empty)
            .WithMessage(ErrorMessages.NewsletterSubscriptionAction_Token_NotEmpty);

        RuleFor(x => x.ConfirmationExpiry)
            .NotNull()
            .WithMessage(ErrorMessages.NewsletterSubscriptionAction_Confirmation_NotNull)
            .Must(x => x.ToUniversalTime() <= DateTime.UtcNow.AddDays(7))
            .WithMessage(ErrorMessages.NewsletterSubscriptionAction_Confirmation_AtMost7DaysInFuture);

        RuleFor(x => x.IsConfirmed)
            .NotNull()
            .WithMessage(ErrorMessages.NewsletterSubscriptionAction_IsConfirmed_NotNull);

        RuleFor(x => x.ConfirmationTime)
            .Must((x, y) =>
            {
                if (y is not null)
                {
                    return y.Value.ToUniversalTime() <= x.ConfirmationExpiry;
                }

                return true;
            })
            .WithMessage(ErrorMessages.NewsletterSubscriptionAction_ConfirmationTime_NotAfterExpiry);
    }
}
