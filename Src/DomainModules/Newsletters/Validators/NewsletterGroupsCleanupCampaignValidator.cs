using DomainModules.Emails.Entities;
using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using FluentValidation;

namespace DomainModules.Newsletters.Validators;

public class NewsletterGroupsCleanupCampaignValidator : AbstractValidator<NewsletterGroupsCleanupCampaign>
{
    public NewsletterGroupsCleanupCampaignValidator(IValidator<Recipient> recipientValidator)
    {
        RuleForEach(x => x.UnconfirmedRecipients).SetValidator(recipientValidator);

        RuleFor(x => x.CampaignStart)
            .Must(x => x.ToUniversalTime() >= DateTime.UtcNow)
            .WithMessage(ErrorMessages.NewsletterGroupsCleanupCampaign_CampaignStart_MustNotBeInThePast);

        RuleFor(x => x.CampaignDurationMonths)
            .GreaterThanOrEqualTo(2)
            .WithMessage(ErrorMessages.NewsletterGroupsCleanupCampaign_CampaignDurationMonths_InvalidDuration);
    }
}
