using DomainModules.Emails.Entities;
using DomainModules.Emails.Validators;
using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using DomainModules.Newsletters.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Newsletters;

[TestFixture, Category("UnitTests")]
public class NewsletterTests
{
    private NewsletterValidator _validator;
    private Newsletter _baseValidNewsletter;

    [SetUp]
    public void SetUp()
    {
        // Set up dependency validators
        var recipientValidator = new RecipientValidator();
        var contactListValidator = new ContactListValidator(recipientValidator);
        var newsletterGroupValidator = new NewsletterGroupValidator(contactListValidator);
        var attachmentValidator = new AttachmentValidator();
        var emailValidator = new EmailValidator(recipientValidator, attachmentValidator);

        _validator = new NewsletterValidator(emailValidator, newsletterGroupValidator);

        _baseValidNewsletter = new Newsletter
        {
            Email = new Email
            {
                SenderEmailAddress = "nyhedsbrev@ommelsamvirke.com",
                Subject = "Test Newsletter Email",
                HtmlBody = "This is a test newsletter email.",
                PlainTextBody = "This is a test newsletter email.",
                Attachments = [],
                Recipients = [ new Recipient { EmailAddress = "test@example.com" } ]
            },
            NewsletterGroups = [
                new NewsletterGroup
                {
                    Name = "Valid Group",
                    Description = "A valid newsletter group description.",
                    ContactList = new ContactList
                    {
                        Name = "Valid Contact List",
                        Description = "A valid contact list description.",
                        Contacts = [ new Recipient { EmailAddress = "test@example.com" } ]
                    }
                }
            ]
        };
    }

    [Test]
    public void ValidNewsletter_PassesValidation()
    {
        Newsletter newsletter = _baseValidNewsletter;
        ValidationResult? result = _validator.Validate(newsletter);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Newsletter_InvalidEmail_FailsValidation()
    {
        Newsletter newsletter = _baseValidNewsletter;
        // Introduce an invalid email by using a sender address that is not approved
        newsletter.Email.SenderEmailAddress = "invalid@example.com";
        var result = _validator.Validate(newsletter);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_SenderAddress_MustBeApproved) ||
                x.ErrorMessage.Equals(ErrorMessages.Email_SenderAddress_MustNotBeEmpty)
            ), Is.True);
        });
    }

    [Test]
    public void Newsletter_InvalidNewsletterGroup_FailsValidation()
    {
        Newsletter newsletter = _baseValidNewsletter;
        // Make the newsletter group invalid by setting an insufficient Name length
        newsletter.NewsletterGroups[0].Name = "ab";
        ValidationResult? result = _validator.Validate(newsletter);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterGroup_Name_InvalidLength)
            ), Is.True);
        });
    }
}
