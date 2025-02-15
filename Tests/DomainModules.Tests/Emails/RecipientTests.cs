using FluentValidation.Results;
using DomainModules.Emails.Entities;
using DomainModules.Emails.Validators;
using DomainModules.Errors;

namespace DomainModules.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class RecipientTests
{
    private RecipientValidator _validator;
    
    [SetUp]
    public void SetUp()
    {
        _validator = new RecipientValidator();
    }

    [TestCase("test@example.com")]
    [TestCase("test@example.uk.co")]
    [TestCase("test-mail@example.com")]
    [TestCase("firstname.lastname@example.com")]
    [TestCase("firstname+lastname@example.com")]
    [TestCase("email@123.123.123.123")]
    [TestCase("\"email\"@example.com")]
    public void EmailAddress_IsValid_PassesValidation(string emailAddress)
    {
        var recipient = new Recipient
        {
            EmailAddress = emailAddress
        };

        ValidationResult validationResult = _validator.Validate(recipient);
        
        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [TestCase("")]
    [TestCase("plainaddress")]
    [TestCase("plainaddress.com")]
    [TestCase("@example.com")]
    [TestCase("test  @example.com")]
    [TestCase("#@%^%#$@#$@#.com")]
    [TestCase(".email@example.com")]
    [TestCase("email@example..com")]
    [TestCase("email@.example.com")]
    [TestCase("Abc..123@example.com")]
    [TestCase(null)]
    public void EmailAddress_IsInvalid_FailsValidation(string? emailAddress)
    {
        var recipient = new Recipient
        {
            EmailAddress = emailAddress!
        };

        ValidationResult validationResult = _validator.Validate(recipient);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Recipient_EmailAddress_MustBeValid) ||
                x.ErrorMessage.Equals(ErrorMessages.Recipient_EmailAddress_MustBeValid)
            ), Is.True);
        });
    }
}
