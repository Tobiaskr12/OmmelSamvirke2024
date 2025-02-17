using DomainModules.Emails.Entities;
using DomainModules.Emails.Validators;
using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using DomainModules.Newsletters.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Newsletters;

[TestFixture, Category("UnitTests")]
public class NewsletterGroupTests
{
    private NewsletterGroupValidator _validator;
    private NewsletterGroup _baseValidNewsletterGroup;

    [SetUp]
    public void SetUp()
    {
        var recipientValidator = new RecipientValidator();
        var contactListValidator = new ContactListValidator(recipientValidator);
        _validator = new NewsletterGroupValidator(contactListValidator);

        _baseValidNewsletterGroup = new NewsletterGroup
        {
            Name = "Valid Group Name",
            Description = "This is a valid description for the newsletter group.",
            ContactList = new ContactList
            {
                Name = "Valid Contact List",
                Description = "A valid contact list description.",
                Contacts = [ new Recipient { EmailAddress = "test@example.com" } ]
            }
        };
    }

    [Test]
    public void ValidNewsletterGroup_PassesValidation()
    {
        NewsletterGroup group = _baseValidNewsletterGroup;
        ValidationResult? result = _validator.Validate(group);
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("")]
    [TestCase("ab")]
    public void NewsletterGroup_InvalidName_FailsValidation(string invalidName)
    {
        NewsletterGroup group = _baseValidNewsletterGroup;
        group.Name = invalidName;
        ValidationResult? result = _validator.Validate(group);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterGroup_Name_InvalidLength)
            ), Is.True);
        });
    }

    [TestCase("")]
    [TestCase("abcd")]
    public void NewsletterGroup_InvalidDescription_FailsValidation(string invalidDescription)
    {
        NewsletterGroup group = _baseValidNewsletterGroup;
        group.Description = invalidDescription;
        ValidationResult? result = _validator.Validate(group);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterGroup_Description_InvalidLength)
            ), Is.True);
        });
    }

    [Test]
    public void NewsletterGroup_InvalidContactList_FailsValidation()
    {
        NewsletterGroup group = _baseValidNewsletterGroup;
        // Invalidate the contact list by providing an invalid Name
        group.ContactList.Name = "";
        ValidationResult? result = _validator.Validate(group);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ContactList_Name_InvalidLength)
            ), Is.True);
        });
    }
}
