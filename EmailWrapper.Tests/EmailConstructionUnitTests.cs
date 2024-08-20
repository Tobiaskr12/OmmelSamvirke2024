using System.Net.Mail;
using EmailWrapper.Constants;
using EmailWrapper.Models;
using FluentResults;

namespace EmailWrapper.Tests;

public class EmailConstructionUnitTests
{
    private Recipient _recipient;
        
    [SetUp]
    public void Setup()
    {
        _recipient = new Recipient
        {
            Email = "test@example.com"
        };
    }
    
    [TestCase(0, TestName  = "Using a subject of length 0 should be invalid", ExpectedResult = false)]
    [TestCase(1, TestName  = "Using a subject of length of at least 1 should be valid", ExpectedResult = true)]
    [TestCase(80, TestName  = "Using a subject of length of up to 80 should be valid", ExpectedResult = true)]
    [TestCase(81, TestName  = "Using a subject of length 81 and above should be invalid", ExpectedResult = false)]
    public bool GivenEmailSubjectOfDifferentLengths_WhenCreatingEmail_CheckValidity(int characterCount)
    {
        Result<Email> result = Email.Create(SenderEmailAddresses.Admins, new string('a', characterCount), "body", _recipient);
        return result.IsSuccess;
    }
    
    // This test assumes UTF-16 encoding
    [TestCase(0, TestName = "Using a body length of 0 should be invalid", ExpectedResult = false)]
    [TestCase(1, TestName = "Using a body with a least 1 character should be valid", ExpectedResult = true)]
    [TestCase(19.9 * 1024 * 1024 / 2, TestName = "Using a body with a size of almost 20MB should be valid", ExpectedResult = true)]
    [TestCase(20.1 * 1024 * 1024 / 2, TestName = "Using a body with a size greater than 20MB should be invalid", ExpectedResult = false)]
    public bool GivenEmailWithDifferentBodyLengths_WhenCreatingEmail_CheckValidity(double characterCount)
    {
        Result<Email> result = Email.Create(
            SenderEmailAddresses.Admins,
            "subject",
            new string('a', (int)characterCount),
            _recipient);
        return result.IsSuccess;
    }

    [TestCase("auto@ommelsamvirke.com", TestName = "auto@ommelsamvirke.com should be valid", ExpectedResult = true)]
    [TestCase("admins@ommelsamvirke.com", TestName = "admins@ommelsamvirke.com should be valid", ExpectedResult = true)]
    [TestCase("auth@ommelsamvirke.com", TestName = "auth@ommelsamvirke.com should be valid", ExpectedResult = true)]
    [TestCase("nyhedsbrev@ommelsamvirke.com", TestName = "nyhedsbrev@ommelsamvirke.com should be valid", ExpectedResult = true)]
    [TestCase("invalid@ommelsamvirke.com", TestName = "Email address with invalid value before the '@' should be invalid ", ExpectedResult = false)]
    [TestCase("auto@nonommelsamvirke.com", TestName = "Email address with an invalid doamin name should be invalid", ExpectedResult = false)]
    public bool GivenEmailWithDifferentSenderAddresses_WhenCreatingEmail_CheckValidity(string senderAddress)
    {
        Result<Email> result = Email.Create(
            senderAddress,
            "subject",
            "This is a test body",
            _recipient);
        return result.IsSuccess;
    }
        
    [Test]
    public void GivenEmailWithoutRecipient_WhenCreatingEmail_ReturnsError()
    {
        Result<Email> result = Email.Create(SenderEmailAddresses.Admins,"subject", "body", recipients: null);
        Assert.That(result.IsSuccess, Is.False);
    }
        
    [Test]
    public void GivenEmailHasAnAttachmentLargerThan20MBs_WhenCreatingEmail_ReturnsError()
    {
        var largeAttachment = new Attachment(new MemoryStream(new byte[21 * 1024 * 1024]), "large.txt");
        Result<Email> result = Email.Create(
            SenderEmailAddresses.Admins,
            "subject",
            "body",
            _recipient,
            new List<Attachment>
        {
            largeAttachment
        });
        Assert.That(result.IsSuccess, Is.False);
    }
        
    [Test]
    public void GivenEmailHasAttachmentsLargerThan20MBsCombined_WhenCreatingEmail_ReturnsError()
    {
        var attachment1 = new Attachment(new MemoryStream(new byte[10 * 1024 * 1024]), "file1.txt");
        var attachment2 = new Attachment(new MemoryStream(new byte[11 * 1024 * 1024]), "file2.txt");
        Result<Email> result = Email.Create(SenderEmailAddresses.Admins,"subject", "body", _recipient, new List<Attachment>
        {
            attachment1, attachment2
        });
        Assert.That(result.IsSuccess, Is.False);
    }
        
    [Test]
    public void GivenEmailComponentsAreValid_WhenCreatingEmail_ReturnsOk()
    {
        Result<Email> result = Email.Create(
            SenderEmailAddresses.Admins,
            "subject",
            "body",
            _recipient);
        Assert.That(result.IsSuccess, Is.True);
    }
        
    [Test]
    public void GivenEmailIsValidAndHasAValidAttachment_WhenCreatingEmail_ReturnsOk()
    {
        var attachment = new Attachment(new MemoryStream(new byte[5 * 1024 * 1024]), "file.txt");
        Result<Email> result = Email.Create(
            SenderEmailAddresses.Admins,
            "subject",
            "body",
            _recipient,
            new List<Attachment>
        {
            attachment
        });
        Assert.That(result.IsSuccess, Is.True);
    }
        
    [Test]
    public void GivenEmailIsValidAndHasMultipleValidAttachments_WhenCreatingEmail_ReturnsOk()
    {
        var attachment1 = new Attachment(new MemoryStream(new byte[5 * 1024 * 1024]), "file1.txt");
        var attachment2 = new Attachment(new MemoryStream(new byte[4 * 1024 * 1024]), "file2.txt");
        Result<Email> result = Email.Create(
            SenderEmailAddresses.Admins,
            "subject",
            "body",
            _recipient,
            new List<Attachment>
        {
            attachment1, attachment2
        });
        Assert.That(result.IsSuccess, Is.True);
    }
}
