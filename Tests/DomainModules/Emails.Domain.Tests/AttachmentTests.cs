using System.Net.Mime;
using Emails.Domain.Entities;
using Emails.Domain.Errors;
using Emails.Domain.Validators;
using FluentValidation.Results;

namespace Emails.Domain.Tests;

[TestFixture, Category("UnitTests")]
public class AttachmentTests
{
    private AttachmentValidator _validator;
    private Attachment _baseValidAttachment;
    
    private const int OneMb = 1024 * 1024;
    
    [SetUp]
    public void SetUp()
    {
        _validator = new AttachmentValidator();
        _baseValidAttachment = new Attachment
        {
            Name = "Test Attachment",
            ContentPath = new Uri("https://example.com"),
            ContentType = new ContentType("application/pdf")
        };
    }

    [TestCase(5)]
    [TestCase(256)]
    public void Name_ValidLength_PassesValidation(int nameLength)
    {
        Attachment attachment = _baseValidAttachment;
        attachment.Name = new string('a', nameLength);

        ValidationResult validationResult = _validator.Validate(attachment);
        
        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [TestCase(0)]
    [TestCase(4)]
    [TestCase(257)]
    public void Name_InvalidLength_FailsValidationWithExpectedErrorMessage(int nameLength)
    {
        Attachment attachment = _baseValidAttachment;
        attachment.Name = new string('a', nameLength);

        ValidationResult validationResult = _validator.Validate(attachment);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Attachment_Name_InvalidLength)
            ));
        });
    }
    
    [TestCase((int)(OneMb * 7.5))]
    public void BinaryContent_ValidSize_PassesValidation(int attachmentSize)
    {
        Attachment attachment = _baseValidAttachment;
        attachment.BinaryContent = CreateBinaryContent(attachmentSize);

        ValidationResult validationResult = _validator.Validate(attachment);
        
        Assert.That(validationResult.IsValid, Is.True);
    }
    
    [Test]
    public void BinaryContent_IsEmpty_FailsValidationWithExpectedErrorMessage()
    {
        Attachment attachment = _baseValidAttachment;
        attachment.BinaryContent = CreateBinaryContent(0);

        ValidationResult validationResult = _validator.Validate(attachment);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Attachment_BinaryContent_IsEmpty)
            ));
        });
    }

    [Test]
    public void BinaryContent_SizeTooLarge_FailsValidationWithExpectedErrorMessage()
    {
        Attachment attachment = _baseValidAttachment;
        attachment.BinaryContent = CreateBinaryContent((int)(OneMb * 7.51));

        ValidationResult validationResult = _validator.Validate(attachment);
        
        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Attachment_BinaryContent_TooLarge)
            ));
        });
    }
    
    private static byte[] CreateBinaryContent(int contentSize)
    {
        Random randomGen = new();
        var content = new byte[contentSize];
        randomGen.NextBytes(content);

        return content;
    }
}
