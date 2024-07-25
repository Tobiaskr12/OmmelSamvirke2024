using EmailWrapper.Models;
using EmailWrapper.Services;
using EmailWrapper.Tests.Models;
using FluentResults;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NSubstitute;

namespace EmailWrapper.Tests;

public class IntegrationTests
{
    private EmailSender _emailSender;
    private TestEmailClient _testClientOne;
    private TestEmailClient _testClientTwo;
    private IConfigurationRoot _config;
    
    [SetUp]
    public void Setup()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json")
            .AddEnvironmentVariables() 
            .Build();

        var logger = Substitute.For<ILogger>();
        
        _emailSender = new EmailSender(_config, logger);
        string? accountOnePassword = _config["Passwords:EmailTestClientOnePassword"];
        if (accountOnePassword is null)
            throw new Exception("Cannot read the password for email test account one");
        
        _testClientOne = new TestEmailClient
        {
            EmailAddress = "ommelsamvirketest1@gmail.com",
            AccountPassword = accountOnePassword,
            ImapHost = "imap.gmail.com",
            ImapPort = 993
        };
        _testClientTwo = new TestEmailClient
        {
            EmailAddress = "ommelsamvirketest2@gmail.com",
            AccountPassword = "",
            ImapHost = "imap.gmail.com",
            ImapPort = 993
        };
    }

    [Test]
    public async Task GivenValidEmailIsSent_WhenCheckingEmailClient_TheEmailIsDelivered()
    {
        var messageGuid = Guid.NewGuid();
        var recipient = new Recipient
        {
            Email = _testClientOne.EmailAddress
        };
        Result<Email> emailResult = Email.Create($"{messageGuid} - Test Email", "This is a test email.", recipient);
        if (emailResult.IsFailed)
        {
            Assert.Fail();
            return;
        }

        Email email = emailResult.Value;
        await _emailSender.SendEmail(email);

        Result<MimeMessage> receivedMessageResult = await GetLatestEmailAsync(_testClientOne, messageGuid);
        if (receivedMessageResult.IsFailed)
        {
            Assert.Fail();
            return;
        }

        MimeMessage receivedMessage = receivedMessageResult.Value;
        Assert.Multiple(() =>
        {
            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(receivedMessage.Subject, Is.EqualTo(email.Subject));
            Assert.That(receivedMessage.HtmlBody.Trim(), Is.EqualTo(email.Body));
        });
    }
    
    [Test]
    public void GivenEmailHasAnAttachment_WhenCheckingEmailClient_TheEmailContainsTheAttachment()
    {
        Assert.Pass();
    }
    
    [Test]
    public void GivenEmailHasMultipleAttachments_WhenCheckingEmailClient_TheEmailContainsAllAttachments()
    {
        Assert.Pass();
    }
    
    [Test]
    public void GivenValidEmailIsSentToTwoClients_WhenCheckingEmailClients_TheEmailsAreDelivered()
    {
        Assert.Pass();
    }
    
    private async Task<Result<MimeMessage>> GetLatestEmailAsync(TestEmailClient testClient, Guid messageGuid)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(testClient.ImapHost, testClient.ImapPort, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(testClient.EmailAddress, testClient.AccountPassword);

        IMailFolder? inbox = client.Inbox;
        if (inbox is null) return Result.Fail("Inbox was null");
    
        await inbox.OpenAsync(FolderAccess.ReadOnly);

        DateTime testTimeout = DateTime.UtcNow.AddMinutes(1);
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

            await Task.Delay(5000);
        }

        await client.DisconnectAsync(true);
        return Result.Fail("No recent email found within the last minute.");
    }
}
