using System.Net.Mime;
using System.Text;
using FluentValidation.Results;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Emails.Validators;
using OmmelSamvirke.DomainModules.Errors;

namespace OmmelSamvirke.DomainModules.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class EmailTests
{
    private EmailValidator _validator;
    private Email _baseValidEmail;

    private const int OneMb = 1024 * 1024;
    
    [SetUp]
    public void SetUp()
    {
        var recipientValidator = new RecipientValidator();
        var attachmentValidator = new AttachmentValidator();
        
        _validator = new EmailValidator(recipientValidator, attachmentValidator);
        _baseValidEmail = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Subject = "This is a test email",
            Body = "This is the body content of a test email",
            Attachments = [],
            Recipients = [new Recipient
            {
                EmailAddress = "test@example.com"
            }]
        };
    }
    
    [TestCase("auto@ommelsamvirke.com")]
    [TestCase("admins@ommelsamvirke.com")]
    [TestCase("auth@ommelsamvirke.com")]
    [TestCase("nyhedsbrev@ommelsamvirke.com")]
    public void SenderEmailAddress_IsValid_PassesValidation(string fromEmailAddress)
    {
        Email email = _baseValidEmail;
        email.SenderEmailAddress = fromEmailAddress;

        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [TestCase("auto@example.com")]
    [TestCase("random@example.com")]
    [TestCase("invalid@ommelsamvirke.com")]
    [TestCase("")]
    [TestCase(null)]
    public void SenderEmailAddress_IsInvalid_FailsValidation(string? fromEmailAddress)
    {
        Email email = _baseValidEmail;
        email.SenderEmailAddress = fromEmailAddress!;

        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_SenderAddress_MustBeApproved) ||
                x.ErrorMessage.Equals(ErrorMessages.Email_SenderAddress_MustNotBeEmpty)
            ), Is.True);
        });
    }

    [TestCase(3)]
    [TestCase(80)]
    public void Subject_ValidLength_PassesValidation(int subjectLength)
    {
        Email email = _baseValidEmail;
        email.Subject = new string('a', subjectLength);
        
        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [TestCase(0)]
    [TestCase(2)]
    [TestCase(81)]
    public void Subject_InvalidLength_FailsValidationWithExpectedErrorMessage(int subjectLength)
    {
        Email email = _baseValidEmail;
        email.Subject = new string('a', subjectLength);
    
        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_Subject_InvalidLength)
            ));
        });
    }
    
    // This test assumes UTF-16 encoding
    [TestCase(20)]
    [TestCase((int)(6.9 * OneMb / 2))]
    public void Body_ValidLength_PassesValidation(int bodyLength)
    {
        Email email = _baseValidEmail;
        email.Body = new string('a', bodyLength);
        
        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }
    
    // This test assumes UTF-16 encoding
    [TestCase(0)]
    [TestCase(19)]
    [TestCase((int)(7.1 * OneMb / 2))]
    public void Body_InvalidLength_FailsValidationWithExpectedErrorMessage(int subjectLength)
    {
        Email email = _baseValidEmail;
        email.Body = new string('a', subjectLength);
    
        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_Body_InvalidLength)
            ));
        });
    }
    
    [TestCase(1)]
    [TestCase(50)]
    public void Recipients_HasValidSize_PassesValidation(int numberOfRecipients)
    {
        List<Recipient> recipients = CreateRecipients(numberOfRecipients);
        Email email = _baseValidEmail;
        email.Recipients = recipients;
        
        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [TestCase(0)]
    [TestCase(51)]
    public void Recipient_InvalidSize_FailsValidationWithExpectedErrorMessage(int numberOfRecipients)
    {
        List<Recipient> recipients = CreateRecipients(numberOfRecipients);
        Email email = _baseValidEmail;
        email.Recipients = recipients;
    
        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_Recipient_InvalidSize)
            ));
        });
    }
    
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void Attachments_HasValidSize_PassesValidation(int numberOfAttachments)
    {
        List<Attachment> attachments = CreateAttachments(numberOfAttachments, 10_000);
        Email email = _baseValidEmail;
        email.Attachments = attachments;
        
        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [TestCase(11)]
    public void Attachments_InvalidSize_FailsValidationWithExpectedErrorMessage(int numberOfAttachments)
    {
        List<Attachment> attachments = CreateAttachments(numberOfAttachments, 10_000);
        Email email = _baseValidEmail;
        email.Attachments = attachments;
    
        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_Attachments_InvalidSize)
            ));
        });
    }

    [TestCase(0.5)]
    [TestCase(1)]
    [TestCase(7.5)]
    public void Email_HasValidContentSize_PassesValidation(double contentSizeMb)
    {
        Email email = _baseValidEmail;
        var attachmentContentSize =
            (int)(contentSizeMb * OneMb - Encoding.Unicode.GetByteCount(email.Subject + email.Body));
        email.Attachments.Add(CreateAttachmentOfSize(attachmentContentSize));
        
        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [TestCase(7.6)]
    public void Email_HasInvalidContentSize_FailsValidationWithExpectedErrorMessage(double contentSizeMb)
    {
        Email email = _baseValidEmail;
        var attachmentContentSize =
            (int)(contentSizeMb * OneMb - Encoding.Unicode.GetByteCount(email.Subject + email.Body));
        email.Attachments.Add(CreateAttachmentOfSize(attachmentContentSize));
        
        ValidationResult validationResult = _validator.Validate(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_ContentSize_TooLarge)
            ));
        });
    }

    private List<Recipient> CreateRecipients(int count)
    {
        _baseValidEmail.Recipients.Clear();
        return Enumerable.Repeat(new Recipient
        {
            EmailAddress = "test@example.com"
        }, count).ToList();
    }

    private static List<Attachment> CreateAttachments(int count, int size)
    {
        return Enumerable.Repeat(CreateAttachmentOfSize(size), count).ToList();
    } 
    
    private static Attachment CreateAttachmentOfSize(int sizeInBytes)
    {
        Random randomGen = new();
        var binaryContent = new byte[sizeInBytes];
        randomGen.NextBytes(binaryContent);

        return new Attachment
        {
            Name = Guid.NewGuid().ToString(),
            ContentPath = new Uri("https://example.com"),
            ContentType = new ContentType("application/pdf"),
            BinaryContent = binaryContent
        };
    }
}