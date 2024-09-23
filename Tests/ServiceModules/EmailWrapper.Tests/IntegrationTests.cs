// using System.Net.Mail;
// using Emails.Services.Constants;
// using Emails.Services.Interfaces;
// using Emails.Services.Models;
// using Emails.Services.Services;
// using Emails.Services.Tests.Models;
// using FluentResults;
// using MailKit;
// using MailKit.Net.Imap;
// using MailKit.Search;
// using MailKit.Security;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using MimeKit;
// using NSubstitute;
// using SecretsManager;
//
// namespace Emails.Services.Tests;
//
// public class IntegrationTests
// {
//     private IEmailSender _emailSender;
//     private TestEmailClient _testClientOne = null!;
//     private TestEmailClient _testClientTwo = null!;
//     private IConfigurationRoot _config;
//     
//     private static IEnumerable<string> SenderEmailAddressesSource => new[]
//     {
//         SenderEmailAddresses.Admins,
//         SenderEmailAddresses.Auto,
//         SenderEmailAddresses.Auth,
//         SenderEmailAddresses.Newsletter
//     };
//
//     [SetUp]
//     public void Setup()
//     {
//         _config = new ConfigurationBuilder()
//             .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
//             .Build();
//
//         var logger = Substitute.For<ILogger>();
//         _emailSender = new EmailSender(_config, logger);
//
//         SetupEmailTestAccount(
//             testClient: out _testClientOne,
//             emailAddress: "ommelsamvirketest1@gmail.com",
//             passwordConfigSection: "EmailTestClientOneAppPassword"
//         );
//
//         SetupEmailTestAccount(
//             testClient: out _testClientTwo,
//             emailAddress: "ommelsamvirketest2@gmail.com",
//             passwordConfigSection: "EmailTestClientTwoAppPassword"
//         );
//     }
//
//     [TestCaseSource(nameof(SenderEmailAddressesSource))]
//     public async Task GivenValidEmailIsSent_WhenCheckingEmailClient_TheEmailIsDelivered(string senderEmailAddress)
//     {
//         var messageGuid = Guid.NewGuid();
//         Email email = CreateAndSendEmail(senderEmailAddress, "Test Email", _testClientOne.EmailAddress, messageGuid);
//         MimeMessage? receivedMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
//
//         AssertEmailReceived(receivedMessage, email);
//     }
//
//     [Test]
//     public async Task GivenValidEmailIsSentToTwoClients_WhenCheckingEmailClients_TheEmailsAreDelivered()
//     {
//         var messageGuid = Guid.NewGuid();
//         List<Recipient> recipients = Recipient.Create([_testClientOne.EmailAddress, _testClientTwo.EmailAddress]);
//         var email = CreateAndSendEmail(SenderEmailAddresses.Admins, "Test Email", recipients, messageGuid);
//
//         var testClientOneMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
//         var testClientTwoMessage = await ExtractLatestReceivedMessageFromInbox(_testClientTwo, messageGuid);
//
//         AssertEmailReceived(testClientOneMessage, email);
//         AssertEmailReceived(testClientTwoMessage, email);
//     }
//
//     [TestCase(1, "./TestDocuments/Test_PDF1.pdf")]
//     [TestCase(2, "./TestDocuments/Test_PDF1.pdf", "./TestDocuments/Test_PDF2.pdf")]
//     public async Task GivenEmailHasAttachments_WhenCheckingEmailClient_TheEmailContainsAllAttachments(int attachmentCount, params string[] attachmentPaths)
//     {
//         var messageGuid = Guid.NewGuid();
//         Email email = CreateAndSendEmailWithAttachments(SenderEmailAddresses.Admins, "Test Email with Attachments", _testClientOne.EmailAddress, messageGuid, attachmentPaths);
//
//         MimeMessage? receivedMessage = await ExtractLatestReceivedMessageFromInbox(_testClientOne, messageGuid);
//         AssertEmailReceived(receivedMessage, email);
//
//         Assert.Multiple(() =>
//         {
//             List<MimePart>? receivedAttachments = receivedMessage?.BodyParts.OfType<MimePart>().Where(bp => bp.IsAttachment).ToList();
//             Assert.That(receivedAttachments!.Count, Is.EqualTo(attachmentCount));
//             for (var i = 0; i < attachmentCount; i++)
//             {
//                 AssertAttachment(receivedAttachments[i], email.Attachments[i]);
//             }
//         });
//     }
//
//     private Email CreateAndSendEmail(string senderEmailAddress, string emailSubjectSuffix, string recipientEmail, Guid messageGuid)
//     {
//         var recipient = new Recipient { Email = recipientEmail };
//         Result<Email> emailResult = Email.Create(
//             senderEmailAddress,
//             $"{messageGuid} - {emailSubjectSuffix}",
//             "This is a test email.",
//             recipient);
//
//         if (emailResult.IsFailed)
//         {
//             Assert.Fail();
//         }
//
//         Email? email = emailResult.Value;
//         _emailSender.SendEmail(email).Wait();
//         return email;
//     }
//
//     private Email CreateAndSendEmail(string senderEmailAddress, string emailSubjectSuffix, List<Recipient> recipients, Guid messageGuid)
//     {
//         Result<Email> emailResult = Email.Create(
//             senderEmailAddress,
//             $"{messageGuid} - {emailSubjectSuffix}",
//             "This is a test email.",
//             recipients);
//
//         if (emailResult.IsFailed)
//         {
//             Assert.Fail();
//         }
//
//         Email? email = emailResult.Value;
//         _emailSender.SendEmail(email).Wait();
//         return email;
//     }
//
//     private Email CreateAndSendEmailWithAttachments(
//         string senderEmailAddress,
//         string emailSubjectSuffix,
//         string recipientEmail,
//         Guid messageGuid,
//         params string[] attachmentPaths)
//     {
//         var recipient = new Recipient { Email = recipientEmail };
//         List<Attachment> attachments = attachmentPaths.Select(path => new Attachment(path)).ToList();
//         Result<Email> emailResult = Email.Create(
//             senderEmailAddress,
//             $"{messageGuid} - {emailSubjectSuffix}",
//             "This is a test email with attachments.",
//             recipient,
//             attachments);
//
//         if (emailResult.IsFailed)
//         {
//             Assert.Fail();
//         }
//
//         Email? email = emailResult.Value;
//         _emailSender.SendEmail(email).Wait();
//         return email;
//     }
//
//     private void SetupEmailTestAccount(out TestEmailClient testClient, string emailAddress, string passwordConfigSection)
//     {
//         string? accountPassword = _config.GetSection(passwordConfigSection).Value;
//         if (accountPassword is null)
//             throw new Exception($"Cannot read the password for the email test account at the config section {passwordConfigSection}");
//
//         testClient = new TestEmailClient
//         {
//             EmailAddress = emailAddress,
//             AccountPassword = accountPassword,
//             ImapHost = "imap.gmail.com",
//             ImapPort = 993
//         };
//     }
//
//     private async Task<MimeMessage?> ExtractLatestReceivedMessageFromInbox(TestEmailClient testEmailClient, Guid messageGuid)
//     {
//         Result<MimeMessage> receivedMessageResult = await GetLatestEmailAsync(testEmailClient, messageGuid);
//         if (receivedMessageResult.IsSuccess) return receivedMessageResult.Value;
//
//         Assert.Fail();
//         return null;
//     }
//
//     private static async Task<Result<MimeMessage>> GetLatestEmailAsync(TestEmailClient testClient, Guid messageGuid)
//     {
//         using var client = new ImapClient();
//         await client.ConnectAsync(testClient.ImapHost, testClient.ImapPort, SecureSocketOptions.SslOnConnect);
//         await client.AuthenticateAsync(testClient.EmailAddress, testClient.AccountPassword);
//
//         IMailFolder? inbox = client.Inbox;
//         if (inbox == null) return Result.Fail("Inbox was null");
//
//         await inbox.OpenAsync(FolderAccess.ReadOnly);
//
//         DateTime testTimeout = DateTime.UtcNow.AddMinutes(1);
//         while (DateTime.UtcNow < testTimeout)
//         {
//             IList<UniqueId>? results = await inbox.SearchAsync(SearchQuery.All);
//             if (results.Any())
//             {
//                 MimeMessage? latestMessage = await inbox.GetMessageAsync(results.Last());
//                 if (latestMessage.Subject.Contains(messageGuid.ToString()))
//                 {
//                     await client.DisconnectAsync(true);
//                     return Result.Ok(latestMessage);
//                 }
//             }
//
//             await Task.Delay(5000);
//         }
//
//         await client.DisconnectAsync(true);
//         return Result.Fail("No recent email found within the last minute.");
//     }
//
//     private static void AssertEmailReceived(MimeMessage? receivedMessage, Email expectedEmail)
//     {
//         Assert.Multiple(() =>
//         {
//             Assert.That(receivedMessage, Is.Not.Null);
//             Assert.That(receivedMessage!.Subject, Is.EqualTo(expectedEmail.Subject));
//             Assert.That(receivedMessage.HtmlBody.Trim(), Is.EqualTo(expectedEmail.Body));
//         });
//     }
//
//     private static void AssertAttachment(MimePart receivedAttachment, Attachment expectedAttachment)
//     {
//         Assert.Multiple(() =>
//         {
//             Assert.That(receivedAttachment.IsAttachment, Is.True);
//             Assert.That(receivedAttachment.ContentType.Name, Is.EqualTo(expectedAttachment.ContentType.Name));
//             Assert.That(receivedAttachment.FileName, Is.EqualTo(expectedAttachment.Name));
//         });
//     }
// }
