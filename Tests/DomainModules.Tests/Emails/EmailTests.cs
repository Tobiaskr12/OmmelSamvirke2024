using System.Text;
using FluentValidation.Results;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Emails.Validators;
using DomainModules.Errors;
using DomainModules.BlobStorage.Entities;
using DomainModules.BlobStorage.Validators;

namespace DomainModules.Tests.Emails;

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
        var fileValidator = new BlobStorageFileValidator();
            
        _validator = new EmailValidator(recipientValidator, fileValidator);
        _baseValidEmail = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Subject = "This is a test email",
            HtmlBody = "This is the body content of a test email",
            PlainTextBody = "This is the body content of a test email",
            Attachments = new List<BlobStorageFile>(),
            Recipients = new List<Recipient>
            {
                new Recipient { EmailAddress = "test@example.com" }
            }
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
                x.ErrorMessage.Equals(ErrorMessages.Email_SenderAddress_MustBeApproved)
                || x.ErrorMessage.Equals(ErrorMessages.Email_SenderAddress_MustNotBeEmpty)
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

    // This test assumes UTF-16 encoding.
    [TestCase(20)]
    [TestCase((int)(6.9 * OneMb / 2))]
    public void Body_ValidLength_PassesValidation(int bodyLength)
    {
        Email email = _baseValidEmail;
        email.HtmlBody = new string('a', bodyLength);

        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }

    // This test assumes UTF-16 encoding.
    [TestCase(0)]
    [TestCase(19)]
    [TestCase((int)(7.1 * OneMb / 2))]
    public void Body_InvalidLength_FailsValidationWithExpectedErrorMessage(int bodyLength)
    {
        Email email = _baseValidEmail;
        email.HtmlBody = new string('a', bodyLength);

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
    [TestCase(ServiceLimits.RecipientsPerEmail)]
    public void Recipients_HasValidSize_PassesValidation(int numberOfRecipients)
    {
        List<Recipient> recipients = CreateRecipients(numberOfRecipients);
        Email email = _baseValidEmail;
        email.Recipients = recipients;

        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [TestCase(0)]
    [TestCase(ServiceLimits.RecipientsPerEmail + 1)]
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
    public void Attachments_HasValidSize_PassesValidation(int numberOfFiles)
    {
        List<BlobStorageFile> files = CreateFiles(numberOfFiles, 10_000);
        Email email = _baseValidEmail;
        email.Attachments = files;

        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [TestCase(11)]
    public void Attachments_InvalidSize_FailsValidationWithExpectedErrorMessage(int numberOfFiles)
    {
        List<BlobStorageFile> files = CreateFiles(numberOfFiles, 10_000);
        Email email = _baseValidEmail;
        email.Attachments = files;

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
        int fileContentSize =
            (int)(contentSizeMb * OneMb - Encoding.Unicode.GetByteCount(email.Subject + email.HtmlBody + email.PlainTextBody));
        email.Attachments.Add(CreateFileOfSize(fileContentSize));

        ValidationResult validationResult = _validator.Validate(email);

        Assert.That(validationResult.IsValid, Is.True);
    }

    [TestCase(7.6)]
    public void Email_HasInvalidContentSize_FailsValidationWithExpectedErrorMessage(double contentSizeMb)
    {
        Email email = _baseValidEmail;
        int fileContentSize =
            (int)(contentSizeMb * OneMb - Encoding.Unicode.GetByteCount(email.Subject + email.HtmlBody + email.PlainTextBody));
        email.Attachments.Add(CreateFileOfSize(fileContentSize));

        ValidationResult validationResult = _validator.Validate(email);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_ContentSize_TooLarge)
            ));
        });
    }

    [Test]
    public void Email_HasDuplicateRecipients_FailsValidationWithExpectedErrorMessage()
    {
        string duplicatedEmailAddress = "testemail@example.com";
        List<Recipient> recipients = CreateRecipients(3);
        recipients[0].EmailAddress = duplicatedEmailAddress;
        recipients[1].EmailAddress = duplicatedEmailAddress;

        Email email = _baseValidEmail;
        email.Recipients = recipients;

        ValidationResult validationResult = _validator.Validate(email);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_Recipients_MustBeUnique)
            ));
        });
    }

    [Test]
    public void Email_HasDuplicateAttachments_FailsValidationWithExpectedErrorMessage()
    {
        string duplicatedFileName = "testfile";
        List<BlobStorageFile> files = CreateFiles(3, 2000);
        files[0].FileBaseName = duplicatedFileName;
        files[1].FileBaseName = duplicatedFileName;

        Email email = _baseValidEmail;
        email.Attachments = files;

        ValidationResult validationResult = _validator.Validate(email);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Email_Attachments_MustBeUnique)
            ));
        });
    }

    // Helper method to create a list of Recipient objects.
    private List<Recipient> CreateRecipients(int count)
    {
        _baseValidEmail.Recipients.Clear();
        var recipients = new List<Recipient>();

        for (int i = 0; i < count; i++)
        {
            recipients.Add(new Recipient()
            {
                EmailAddress = $"test{i}@example.com",
            });
        }

        return recipients;
    }

    // Helper method to create a list of BlobStorageFile objects.
    private static List<BlobStorageFile> CreateFiles(int count, int sizeInBytes)
    {
        var files = new List<BlobStorageFile>();

        for (int i = 0; i < count; i++)
        {
            files.Add(CreateFileOfSize(sizeInBytes));
        }

        return files;
    }

    // Helper method to create a BlobStorageFile with file content of specified size.
    private static BlobStorageFile CreateFileOfSize(int sizeInBytes)
    {
        Random randomGen = new();
        byte[] binaryContent = new byte[sizeInBytes];
        randomGen.NextBytes(binaryContent);

        return new BlobStorageFile
        {
            FileBaseName = Guid.NewGuid().ToString(),
            FileExtension = "pdf",
            ContentType = "application/pdf",
            FileContent = new MemoryStream(binaryContent)
        };
    }
}