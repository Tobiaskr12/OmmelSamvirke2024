using FluentResults;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Sending.Commands.E2E;

[TestFixture, Category("IntegrationTests")]
public class EmailSendingTests
{
    private TestEmailClient _testClientOne = null!;
    private TestEmailClient _testClientTwo = null!;
    private const string BaseTestDocumentsPath = "./Emails/Sending/Commands/E2E/TestDocuments/";
    private IntegrationTestingHelper _integrationTestingHelper;
    
    private static IEnumerable<string> SenderEmailAddressesSource =>
    [
        ValidSenderEmailAddresses.Admins,
        ValidSenderEmailAddresses.Auto,
        ValidSenderEmailAddresses.Auth,
        ValidSenderEmailAddresses.Newsletter
    ];

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
        
        SetupEmailTestAccount(
            testClient: out _testClientOne,
            emailAddress: "ommelsamvirketest1@gmail.com",
            passwordConfigSection: "EmailTestClientOneAppPassword"
        );

        SetupEmailTestAccount(
            testClient: out _testClientTwo,
            emailAddress: "ommelsamvirketest2@gmail.com",
            passwordConfigSection: "EmailTestClientTwoAppPassword"
        );
    }
    
    [TestCaseSource(nameof(SenderEmailAddressesSource))]
    public async Task GivenValidEmailIsSent_WhenCheckingEmailClient_TheEmailIsDelivered(string senderEmailAddress)
    {
        var messageGuid = Guid.NewGuid();
        Email email = await CreateAndSendEmail(senderEmailAddress, "Test Email", _testClientOne.EmailAddress, messageGuid);
        MimeMessage? receivedMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);

        AssertEmailReceived(receivedMessage, email);
    }

    [Test]
    public async Task GivenValidEmailIsSentToTwoClients_WhenCheckingEmailClients_TheEmailsAreDelivered()
    {
        var messageGuid = Guid.NewGuid();
        List<Recipient> recipients = 
        [
            new() { EmailAddress = _testClientOne.EmailAddress },
            new() { EmailAddress = _testClientTwo.EmailAddress }
        ];
        
        Email email = await CreateAndSendEmail(ValidSenderEmailAddresses.Admins, "Test Email", recipients, messageGuid);

        MimeMessage? testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
        MimeMessage? testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(_testClientTwo, messageGuid);

        AssertEmailReceived(testClientOneMessage, email);
        AssertEmailReceived(testClientTwoMessage, email);
    }

    [TestCase(1, $"{BaseTestDocumentsPath}Test_PDF1.pdf")]
    [TestCase(2, $"{BaseTestDocumentsPath}Test_PDF1.pdf", $"{BaseTestDocumentsPath}Test_PDF2.pdf")]
    public async Task GivenEmailHasAttachments_WhenCheckingEmailClient_TheEmailContainsAllAttachments(int attachmentCount, params string[] attachmentPaths)
    {
        var messageGuid = Guid.NewGuid();
        Email email = await CreateAndSendEmailWithAttachments(ValidSenderEmailAddresses.Admins, "Test Email with Attachments", _testClientOne.EmailAddress, messageGuid, attachmentPaths);

        MimeMessage? receivedMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
        AssertEmailReceived(receivedMessage, email);

        Assert.Multiple(() =>
        {
            List<MimePart>? receivedAttachments = receivedMessage?.BodyParts.OfType<MimePart>().Where(bp => bp.IsAttachment).ToList();
            Assert.That(receivedAttachments!.Count, Is.EqualTo(attachmentCount));
            for (var i = 0; i < attachmentCount; i++)
            {
                AssertAttachment(receivedAttachments[i], email.Attachments[i]);
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
            Body = "This is a test email",
            Recipients = [],
            Attachments = []
        };
        var contactList = new ContactList
        {
            Name = "Test Name",
            Description = "Test Description",
            Contacts = []
        };

        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendEmailToContactListCommand(email, contactList));
        
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
                new Recipient { EmailAddress = _testClientOne.EmailAddress },
                new Recipient { EmailAddress = _testClientTwo.EmailAddress }
            ]
        };
        
         Email email = await CreateAndSendEmailToContactListWithAttachments(ValidSenderEmailAddresses.Admins, "Test Email with Attachments", messageGuid, contactList, batchSize: 10, useBcc: false, attachmentPaths);

         MimeMessage? testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
         MimeMessage? testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(_testClientTwo, messageGuid);

         
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
                new Recipient { EmailAddress = _testClientOne.EmailAddress },
                new Recipient { EmailAddress = _testClientTwo.EmailAddress }
            ]
        };
        
        await CreateAndSendEmailToContactListWithAttachments(ValidSenderEmailAddresses.Admins, "Test Email with Attachments", messageGuid, contactList, batchSize: 10, useBcc: true, attachmentPaths);

        MimeMessage? testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
        MimeMessage? testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(_testClientTwo, messageGuid);
        
        Assert.Multiple(() =>
        {
            Assert.That(testClientOneMessage?.To.Count, Is.EqualTo(1));
            Assert.That(testClientTwoMessage?.To.Count, Is.EqualTo(1));
            Assert.That(testClientOneMessage?.To[0].Name, Is.EqualTo("Undisclosed recipients"));
            Assert.That(testClientOneMessage?.To[0].Name, Is.EqualTo("Undisclosed recipients"));
        });
    }
    
    /// <summary>
    /// This test attempts to send an email to a non-whitelisted email address,
    /// which should not be allowed from the testing environment
    /// </summary>
    [Test]
    public async Task GivenNonWhitelistedRecipient_WhenSendingEmail_TheEmailSendingFails()
    {
        var invalidRecipient = new Recipient { EmailAddress = "invalid@example.com" };
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Admins,
            Subject = "Test Email to Invalid Recipient",
            Body = "This email should not be sent.",
            Recipients = [invalidRecipient],
            Attachments = []
        };

        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendEmailCommand(email));
        
        Assert.That(result.IsFailed);
    }
    
    private async Task<Email> CreateAndSendEmail(string senderEmailAddress, string emailSubjectSuffix, string recipientEmail, Guid messageGuid)
    {
        var recipient = new Recipient { EmailAddress = recipientEmail };
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            Body = "This is a test email",
            Recipients = [recipient],
            Attachments = []
        };
        
        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendEmailCommand(email));
        if (result.IsFailed) throw new Exception("Sending failed");
        return email;
    }
    
    private async Task<Email> CreateAndSendEmail(string senderEmailAddress, string emailSubjectSuffix, List<Recipient> recipients, Guid messageGuid)
    {
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            Body = "This is a test email",
            Recipients = recipients,
            Attachments = []
        };

        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendEmailCommand(email));
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
        List<Attachment> attachments = attachmentPaths.Select(CreateAttachementFromPath).ToList();
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            Body = "This is a test email",
            Recipients = [recipient],
            Attachments = attachments
        };
        
        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendEmailCommand(email));
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
        List<Attachment> attachments = attachmentPaths.Select(CreateAttachementFromPath).ToList();
        var email = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = $"{messageGuid} - {emailSubjectSuffix}",
            Body = "This is a test email",
            Recipients = [],
            Attachments = attachments
        };
        
        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendEmailToContactListCommand(email, contactList, batchSize, useBcc));
        if (result.IsFailed) throw new Exception("Sending failed");

        return email;
    }

    private static Attachment CreateAttachementFromPath(string path)
    {
        var fileInfo = new FileInfo(path);
        byte[] fileBytes = File.ReadAllBytes(path);
        string contentType = MimeMapping.MimeUtility.GetMimeMapping(fileInfo.FullName);
        
        return new Attachment
        {
            Name = fileInfo.Name,
            ContentPath = new Uri("https://example.com/testpdf"),
            ContentType = new System.Net.Mime.ContentType(contentType),
            BinaryContent = fileBytes
        };
    }
    
    private static async Task<MimeMessage?> ExtractLatestReceivedMessageFromInbox(TestEmailClient testEmailClient, Guid messageGuid)
    {
        Result<MimeMessage> receivedMessageResult = await GetLatestEmailAsync(testEmailClient, messageGuid);
        if (receivedMessageResult.IsSuccess) return receivedMessageResult.Value;

        Assert.Fail();
        return null;
    }

    private static async Task<Result<MimeMessage>> GetLatestEmailAsync(TestEmailClient testClient, Guid messageGuid)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(testClient.ImapHost, testClient.ImapPort, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(testClient.EmailAddress, testClient.AccountPassword);

        IMailFolder? inbox = client.Inbox;
        if (inbox == null) return Result.Fail("Inbox was null");

        await inbox.OpenAsync(FolderAccess.ReadOnly);

        DateTime testTimeout = DateTime.UtcNow.AddMinutes(2);
        while (DateTime.UtcNow < testTimeout)
        {
            IList<UniqueId>? results = await inbox.SearchAsync(SearchQuery.All);
            if (results.Any())
            {
                MimeMessage? latestMessage = await inbox.GetMessageAsync(results.Last());
                if (latestMessage.Subject.Contains(messageGuid.ToString()))
                {
                    await client.DisconnectAsync(true);
                    return Result.Ok(latestMessage);
                }
            }

            await Task.Delay(2500);
        }

        await client.DisconnectAsync(true);
        return Result.Fail("No recent email found within the last minute.");
    }

    private static void AssertEmailReceived(MimeMessage? receivedMessage, Email expectedEmail)
    {
        Assert.Multiple(() =>
        {
            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(receivedMessage!.Subject, Is.EqualTo(expectedEmail.Subject));
            Assert.That(receivedMessage.HtmlBody.Trim(), Is.EqualTo(expectedEmail.Body));
        });
    }

    private static void AssertAttachment(MimePart receivedAttachment, Attachment expectedAttachment)
    {
        Assert.Multiple(() =>
        {
            Assert.That(receivedAttachment.IsAttachment, Is.True);
            Assert.That(receivedAttachment.ContentType.MimeType, Is.EqualTo(expectedAttachment.ContentType.MediaType));
            Assert.That(receivedAttachment.FileName, Is.EqualTo(expectedAttachment.Name));
        });
    }

    private void SetupEmailTestAccount(out TestEmailClient testClient, string emailAddress, string passwordConfigSection)
    {
        string? accountPassword = _integrationTestingHelper.Configuration[passwordConfigSection];
        if (accountPassword is null)
            throw new Exception($"Cannot read the password for the email test account at the config section {passwordConfigSection}");

        testClient = new TestEmailClient
        {
            EmailAddress = emailAddress,
            AccountPassword = accountPassword,
            ImapHost = "imap.gmail.com",
            ImapPort = 993
        };
    }
    
    private class TestEmailClient
    {
        public required string EmailAddress { get; init; }
        public required string AccountPassword { get; init; }
        public required string ImapHost { get; init; }
        public required int ImapPort { get; init; }
    }
}
