using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentValidation;

namespace DomainModules.Newsletters.Validators;

public class NewsletterValidator : AbstractValidator<Newsletter>
{
    public NewsletterValidator(IValidator<Email> emailValidator, IValidator<NewsletterGroup> newsletterGroupValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
        RuleForEach(x => x.NewsletterGroups).SetValidator(newsletterGroupValidator);
    }
}
