using FluentValidation.Results;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Emails.Validators;
using OmmelSamvirke.DomainModules.Errors;

namespace OmmelSamvirke.DomainModules.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class ContactListTests
{
    private ContactListValidator _validator;
    private ContactList _baseValidContactList;

    [SetUp]
    public void SetUp()
    {
        var recipientValidator = new RecipientValidator();
        _validator = new ContactListValidator(recipientValidator);
        _baseValidContactList = new ContactList
        {
            Name = "Valid Contact List",
            Description = "This is a valid description for the contact list.",
            Contacts = 
            [
                new Recipient { EmailAddress = "test@example.com" }
            ]
        };
    }

    [TestCase(3)]
    [TestCase(50)]
    [TestCase(200)]
    public void Name_ValidLength_PassesValidation(int nameLength)
    {
        ContactList contactList = _baseValidContactList;
        contactList.Name = new string('a', nameLength);

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(201)]
    public void Name_InvalidLength_FailsValidationWithExpectedErrorMessage(int nameLength)
    {
        ContactList contactList = _baseValidContactList;
        contactList.Name = new string('a', nameLength);

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ContactList_Name_InvalidLength)
            ), Is.True);
        });
    }

    [TestCase(5)]
    [TestCase(100)]
    [TestCase(2000)]
    public void Description_ValidLength_PassesValidation(int descriptionLength)
    {
        ContactList contactList = _baseValidContactList;
        contactList.Description = new string('a', descriptionLength);

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [TestCase(0)]
    [TestCase(4)]
    [TestCase(2001)]
    public void Description_InvalidLength_FailsValidationWithExpectedErrorMessage(int descriptionLength)
    {
        ContactList contactList = _baseValidContactList;
        contactList.Description = new string('a', descriptionLength);

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ContactList_Description_InvalidLength)
            ), Is.True);
        });
    }

    [Test]
    public void Contacts_AllValid_PassesValidation()
    {
        ContactList contactList = _baseValidContactList;
        contactList.Contacts = CreateRecipients(5, isValid: true);

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public void Contacts_ContainsInvalidRecipient_FailsValidation()
    {
        ContactList contactList = _baseValidContactList;
        contactList.Contacts =
        [
            new Recipient { EmailAddress = "valid@example.com" },
            new Recipient { EmailAddress = string.Empty }
        ];

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Recipient_EmailAddress_MustBeValid)
            ), Is.True);
        });
    }

    [Test]
    public void Contacts_EmptyList_PassesValidation()
    {
        ContactList contactList = _baseValidContactList;
        contactList.Contacts = [];

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public void Name_Null_FailsValidationWithExpectedErrorMessage()
    {
        ContactList contactList = _baseValidContactList;
        contactList.Name = null!;

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ContactList_Name_InvalidLength)
            ), Is.True);
        });
    }

    [Test]
    public void Description_Null_FailsValidationWithExpectedErrorMessage()
    {
        ContactList contactList = _baseValidContactList;
        contactList.Description = null!;

        ValidationResult? validationResult = _validator.Validate(contactList);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.ContactList_Description_InvalidLength)
            ), Is.True);
        });
    }

    [Test]
    public void WhenCreatingContactList_UnsubscribeToken_IsValid()
    {
        ContactList contactList = _baseValidContactList;
        
        ValidationResult? validationResult = _validator.Validate(contactList);
        
        Assert.Multiple(() =>
        {
            Assert.That(contactList.UnsubscribeToken, Is.Not.EqualTo(null));
            Assert.That(contactList.UnsubscribeToken, Is.Not.EqualTo(Guid.Empty));
            Assert.That(validationResult.IsValid, Is.True);
        });
    }

    private static List<Recipient> CreateRecipients(int count, bool isValid)
    {
        return Enumerable.Range(1, count).Select(i => new Recipient
        {
            EmailAddress = isValid ? $"valid{i}@example.com" : ""
        }).ToList();
    }
}
