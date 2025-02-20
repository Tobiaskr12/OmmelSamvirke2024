using DomainModules.Emails.Entities;
using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using DomainModules.Newsletters.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Newsletters;

[TestFixture, Category("UnitTests")]
public class NewsletterSubscriptionConfirmationTests
{
    private NewsletterSubscriptionConfirmationValidator _validator;
    private NewsletterSubscriptionConfirmation _baseValidConfirmation;

    [SetUp]
    public void SetUp()
    {
        _validator = new NewsletterSubscriptionConfirmationValidator();
        _baseValidConfirmation = new NewsletterSubscriptionConfirmation
        {
            ConfirmationExpiry = DateTime.UtcNow.AddDays(7),
            IsConfirmed = false,
            ConfirmationTime = null,
            Recipient = new Recipient { EmailAddress = "test@example.com" }
        };
    }

    [Test]
    public void ValidSubscriptionConfirmation_PassesValidation()
    {
        NewsletterSubscriptionConfirmation confirmation = _baseValidConfirmation;
        ValidationResult? result = _validator.Validate(confirmation);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public void ConfirmationExpiry_GreaterThanAllowed_FailsValidation()
    {
        NewsletterSubscriptionConfirmation confirmation = _baseValidConfirmation;
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(8);
        ValidationResult? result = _validator.Validate(confirmation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterSubscriptionAction_Confirmation_AtMost7DaysInFuture)
            ), Is.True);
        });
    }

    [Test]
    public void ConfirmationTime_AfterExpiry_FailsValidation()
    {
        NewsletterSubscriptionConfirmation confirmation = _baseValidConfirmation;
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(1);
        confirmation.ConfirmationTime = DateTime.UtcNow.AddDays(2);
        ValidationResult? result = _validator.Validate(confirmation);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterSubscriptionAction_ConfirmationTime_NotAfterExpiry)
            ), Is.True);
        });
    }
}
