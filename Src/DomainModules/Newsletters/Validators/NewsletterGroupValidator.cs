using DomainModules.Emails.Entities;
using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using FluentValidation;

namespace DomainModules.Newsletters.Validators;

public class NewsletterGroupValidator : AbstractValidator<NewsletterGroup>
{
    public NewsletterGroupValidator(IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ErrorMessages.NewsletterGroup_Name_InvalidLength)
            .Length(3, 150)
            .WithMessage(ErrorMessages.NewsletterGroup_Name_InvalidLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(ErrorMessages.NewsletterGroup_Description_InvalidLength)
            .Length(5, 500)
            .WithMessage(ErrorMessages.NewsletterGroup_Description_InvalidLength);
    }
}
