using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.Sending;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MimeKit;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.Sending.Commands.E2E;

[TestFixture, Category("IntegrationTests")]
public class EmailSendingTests : ServiceTestBase
{
    private const string BaseTestDocumentsPath = "./Emails/Sending/Commands/E2E/TestDocuments/";
        
    private static IEnumerable<string> SenderEmailAddressesSource =>
    [
        ValidSenderEmailAddresses.Admins,
            ValidSenderEmailAddresses.Auto,
            ValidSenderEmailAddresses.Auth,
            ValidSenderEmailAddresses.Newsletter
    ];
        
    [TestCaseSource(nameof(SenderEmailAddressesSource))]
    public async Task GivenValidEmailIsSent_WhenCheckingEmailClient_TheEmailIsDelivered(string senderEmailAddress)
    {
        var messageGuid = Guid.NewGuid();
        Email email = await CreateAndSendEmail(senderEmailAddress, "Test Email", GlobalTestSetup.TestEmailClientOne.EmailAddress, messageGuid);
        MimeMessage? receivedMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientOne, messageGuid);
        AssertEmailReceived(receivedMessage, email);
    }
        
    [Test]
    public async Task GivenValidEmailIsSentToTwoClients_WhenCheckingEmailClients_TheEmailsAreDelivered()
    {
        var messageGuid = Guid.NewGuid();
        List<Recipient> recipients =
        [
            new() { EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress },
            new() { EmailAddress = GlobalTestSetup.TestEmailClientTwo.EmailAddress }
        ];
        Email email = await CreateAndSendEmail(ValidSenderEmailAddresses.Admins, "Test Email", recipients, messageGuid);
        MimeMessage? testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientOne, messageGuid);
        MimeMessage? testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientTwo, messageGuid);
        AssertEmailReceived(testClientOneMessage, email);
        AssertEmailReceived(testClientTwoMessage, email);
    }
        
    [TestCase(1, $"{BaseTestDocumentsPath}Test_PDF1.pdf")]
    [TestCase(2, $"{BaseTestDocumentsPath}Test_PDF1.pdf", $"{BaseTestDocumentsPath}Test_PDF2.pdf")]
    public async Task GivenEmailHasAttachments_WhenCheckingEmailClient_TheEmailContainsAllAttachments(int attachmentCount, params string[] attachmentPaths)
    {
        var messageGuid = Guid.NewGuid();
        Email email = await CreateAndSendEmailWithAttachments(ValidSenderEmailAddresses.Admins, "Test Email with Attachments", GlobalTestSetup.TestEmailClientOne.EmailAddress, messageGuid, attachmentPaths);
        MimeMessage? receivedMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientOne, messageGuid);
        AssertEmailReceived(receivedMessage, email);
        Assert.Multiple(() =>
        {
            List<MimePart>? receivedAttachments = receivedMessage?.BodyParts
                                                                 .OfType<MimePart>()
                                                                 .Where(bp => bp.IsAttachment)
                                                                 .ToList();
            Assert.That(receivedAttachments, Is.Not.Null);
            Assert.That(receivedAttachments, Has.Count.EqualTo(attachmentCount));
            for (int i = 0; i < attachmentCount; i++)
            {
                AssertAttachment(receivedAttachments?[i], email.Attachments[i]);
            }
        });
    }
        
    [Test]
    public async Task GivenContactListIsEmpty_WhenSendingToContactList_ValidationFails()
    {
        var messageGuid = Guid.NewGuid();
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Admins,
            Subject = $"{messageGuid} - Test Email with Attachments",
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Recipients = [],
            Attachments = []
        };
        var contactList = new ContactList
        {
            Name = "Test Name",
            Description = "Test Description",
            Contacts = []
        };
        Result<EmailSendingStatus> result = await GlobalTestSetup.Mediator.Send(new SendEmailToContactListCommand(email, contactList));
        Assert.That(result.IsFailed);
    }
        
    [Test]
    public async Task GivenEmailIsSentViaContactList_AllRecipientsReceiveTheEmail()
    {
        var messageGuid = Guid.NewGuid();
        string[] attachmentPaths = [$"{BaseTestDocumentsPath}Test_PDF1.pdf", $"{BaseTestDocumentsPath}Test_PDF2.pdf"];
        var contactList = new ContactList
        {
            Name = "Test Name",
            Description = "Test Description",
            Contacts =
            [
                new Recipient { EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress },
                new Recipient { EmailAddress = GlobalTestSetup.TestEmailClientTwo.EmailAddress }
            ]
        };
        Email email = await CreateAndSendEmailToContactListWithAttachments(ValidSenderEmailAddresses.Admins, "Test Email with Attachments", messageGuid, contactList, batchSize: 10, useBcc: false, attachmentPaths);
        MimeMessage? testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientOne, messageGuid);
        MimeMessage? testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientTwo, messageGuid);
        Assert.Multiple(() =>
        {
            AssertEmailReceived(testClientOneMessage, email);
            AssertEmailReceived(testClientTwoMessage, email);
            Assert.That(testClientOneMessage?.To.Count, Is.EqualTo(2));
            Assert.That(testClientTwoMessage?.To.Count, Is.EqualTo(2));
        });
    }
        
    [Test]
    public async Task GivenEmailIsSentViaContactListUsingBcc_AllRecipientsReceiveTheEmailAsBccRecipients()
    {
        var messageGuid = Guid.NewGuid();
        string[] attachmentPaths = [$"{BaseTestDocumentsPath}Test_PDF1.pdf", $"{BaseTestDocumentsPath}Test_PDF2.pdf"];
        var contactList = new ContactList
        {
            Name = "Test Name",
            Description = "Test Description",
            Contacts =
            [
                new Recipient { EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress },
                new Recipient { EmailAddress = GlobalTestSetup.TestEmailClientTwo.EmailAddress }
            ]
        };
        await CreateAndSendEmailToContactListWithAttachments(ValidSenderEmailAddresses.Admins, "Test Email with Attachments", messageGuid, contactList, batchSize: 10, useBcc: true, attachmentPaths);
        MimeMessage? testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientOne, messageGuid);
        MimeMessage? testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClientTwo, messageGuid);
        Assert.Multiple(() =>
        {
            Assert.That(testClientOneMessage?.To.Count, Is.EqualTo(1));
            Assert.That(testClientTwoMessage?.To.Count, Is.EqualTo(1));
            Assert.That(testClientOneMessage?.To[0].Name, Is.EqualTo("Undisclosed recipients"));
            Assert.That(testClientTwoMessage?.To[0].Name, Is.EqualTo("Undisclosed recipients"));
        });
    }
        
    [Test]
    public async Task GivenNonWhitelistedRecipient_WhenSendingEmail_TheEmailSendingFails()
    {
        var invalidRecipient = new Recipient { EmailAddress = "invalid@example.com" };
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Admins,
            Subject = "Test Email to Invalid Recipient",
            HtmlBody = "This email should not be sent.",
            PlainTextBody = "This email should not be sent.",
            Recipients = [invalidRecipient],
            Attachments = []
        };
        Result<EmailSendingStatus> result = await GlobalTestSetup.Mediator.Send(new SendEmailCommand(email));
        Assert.That(result.IsFailed);
    }
        
    // Helper methods

    private async Task<Email> CreateAndSendEmail(string senderEmailAddress, string emailSubjectSuffix, string recipientEmail, Guid messageGuid)
    {
        var recipient = new Recipient { EmailAddress = recipientEmail };
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Recipients = [recipient],
            Attachments = []
        };
        Result<EmailSendingStatus> result = await GlobalTestSetup.Mediator.Send(new SendEmailCommand(email));
        if (result.IsFailed) throw new Exception("Sending failed");
        return email;
    }
        
    private async Task<Email> CreateAndSendEmail(string senderEmailAddress, string emailSubjectSuffix, List<Recipient> recipients, Guid messageGuid)
    {
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Recipients = recipients,
            Attachments = []
        };
        Result<EmailSendingStatus> result = await GlobalTestSetup.Mediator.Send(new SendEmailCommand(email));
        if (result.IsFailed) throw new Exception("Sending failed");
        return email;
    }
        
    private async Task<Email> CreateAndSendEmailWithAttachments(
        string senderEmailAddress,
        string emailSubjectSuffix,
        string recipientEmail,
        Guid messageGuid,
        params string[] attachmentPaths)
    {
        var recipient = new Recipient { EmailAddress = recipientEmail };
        List<BlobStorageFile> attachments = attachmentPaths.Select(CreateFileFromPath).ToList();
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Recipients = [recipient],
            Attachments = attachments
        };
        Result<EmailSendingStatus> result = await GlobalTestSetup.Mediator.Send(new SendEmailCommand(email));
        if (result.IsFailed) throw new Exception("Sending failed");
        return email;
    }
        
    private async Task<Email> CreateAndSendEmailToContactListWithAttachments(
        string senderEmailAddress,
        string emailSubjectSuffix,
        Guid messageGuid,
        ContactList contactList,
        int batchSize,
        bool useBcc = false,
        params string[] attachmentPaths)
    {
        List<BlobStorageFile> attachments = attachmentPaths.Select(CreateFileFromPath).ToList();
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Recipients = [], // Will be derived from the contact list.
            Attachments = attachments
        };
        Result<EmailSendingStatus> result = await GlobalTestSetup.Mediator.Send(new SendEmailToContactListCommand(email, contactList, batchSize, useBcc));
        if (result.IsFailed) throw new Exception("Sending failed");
        return email;
    }
        
    private static BlobStorageFile CreateFileFromPath(string path)
    {
        var fileInfo = new FileInfo(path);
        byte[] fileBytes = File.ReadAllBytes(path);
        // Use a MIME mapping utility to determine the content type.
        string contentType = MimeMapping.MimeUtility.GetMimeMapping(fileInfo.FullName);
            
        return new BlobStorageFile
        {
            FileBaseName = fileInfo.Name,
            FileExtension = Path.GetExtension(fileInfo.Name).TrimStart('.'),
            ContentType = contentType,
            FileContent = new MemoryStream(fileBytes)
        };
    }
        
    private async Task<MimeMessage?> ExtractLatestReceivedMessageFromInbox(GlobalTestSetup.TestEmailClient testEmailClient, Guid messageGuid)
    {
        MimeMessage? receivedMessage = await GetLatestEmailAsync(testEmailClient, messageGuid.ToString());
        if (receivedMessage == null)
        {
            Assert.Fail();
            return null;
        }
        Assert.That(receivedMessage.Subject, Does.Contain(messageGuid.ToString()));
        return receivedMessage;
    }
        
    private static void AssertEmailReceived(MimeMessage? receivedMessage, Email expectedEmail)
    {
        Assert.Multiple(() =>
        {
            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(receivedMessage!.Subject, Is.EqualTo(expectedEmail.Subject));
            Assert.That(receivedMessage.HtmlBody.Trim(), Is.EqualTo(expectedEmail.HtmlBody));
        });
    }
        
    private static void AssertAttachment(MimePart? receivedAttachment, BlobStorageFile expectedFile)
    {
        Assert.Multiple(() =>
        {
            Assert.That(receivedAttachment, Is.Not.Null);
            Assert.That(receivedAttachment!.IsAttachment, Is.True);
            Assert.That(receivedAttachment.ContentType.MimeType, Is.EqualTo(expectedFile.ContentType));
            Assert.That(receivedAttachment.FileName, Is.EqualTo($"{expectedFile.FileBaseName}"));
        });
    }
}
