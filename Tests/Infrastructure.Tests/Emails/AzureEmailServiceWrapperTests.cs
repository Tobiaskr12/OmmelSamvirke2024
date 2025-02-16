using System.Net.Mime;
using System.Reflection;
using Azure;
using Azure.Communication.Email;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using DomainModules.Emails.Entities;
using Infrastructure.Emails;

namespace Infrastructure.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class AzureEmailServiceWrapperTests
{
    private IConfiguration _configuration;
    private ILoggingHandler _logger;
    private EmailClient _emailClient;

    private const string ValidConnectionString = "Endpoint=https://example.com/;AccessKey=examplekey";

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILoggingHandler>();

        // Substitute for EmailClient. (Ensure that the members you want to intercept are virtual.)
        _emailClient = Substitute.For<EmailClient>(ValidConnectionString);

        var acsConnectionStringSection = Substitute.For<IConfigurationSection>();
        acsConnectionStringSection.Value.Returns(ValidConnectionString);

        _configuration = Substitute.For<IConfiguration>();
        _configuration.GetSection("AcsConnectionString").Returns(acsConnectionStringSection);
    }

    [Test]
    public async Task SendAsync_EmailClientThrowsException_ReturnsFailureResult()
    {
        // Arrange
        Email email = CreateValidEmail();

        // Simulate SendAsync throwing an exception.
        _emailClient.SendAsync(
            Arg.Any<WaitUntil>(),
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>())
            .Returns<Task<EmailSendOperation>>(_ => Task.FromException<EmailSendOperation>(new Exception("SendAsync failed")));

        var serviceWrapper = new AzureEmailServiceWrapper(_configuration, _logger);
        InjectEmailClient(serviceWrapper, _emailClient);

        // Act
        Result<EmailSendingStatus> result = await serviceWrapper.SendAsync(email);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(Errors.ErrorMessages.AzureEmailSendingFailed));
        });
    }

    [Test]
    public async Task ConvertEmailToAzureEmailMessage_InvalidAttachment_ReturnsFail()
    {
        // Arrange
        Email email = CreateValidEmail();
        email.Attachments.Add(new Attachment
        {
            Name = "test.txt",
            ContentType = new ContentType { Name = "text/plain" },
            ContentPath = new Uri("https://www.test.example.com"),
            BinaryContent = null // Invalid attachment: null content.
        });

        var emailSendOperation = Substitute.For<EmailSendOperation>();
        emailSendOperation.HasValue.Returns(false);

        _emailClient.SendAsync(
            Arg.Any<WaitUntil>(),
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(emailSendOperation));

        var serviceWrapper = new AzureEmailServiceWrapper(_configuration, _logger);
        InjectEmailClient(serviceWrapper, _emailClient);

        // Act
        Result<EmailSendingStatus> result = await serviceWrapper.SendAsync(email);

        // Assert
        Assert.That(result.IsFailed);
    }

    private static Email CreateValidEmail()
    {
        return new Email
        {
            SenderEmailAddress = "sender@example.com",
            Recipients = new List<Recipient>
            {
                new Recipient { EmailAddress = "recipient@example.com" }
            },
            Subject = "Test Email",
            HtmlBody = "<h1>This is a test email</h1>",
            PlainTextBody = "<h1>This is a test email</h1>",
            Attachments = new List<Attachment>()
        };
    }

    private static void InjectEmailClient(AzureEmailServiceWrapper serviceWrapper, EmailClient emailClient)
    {
        FieldInfo? emailClientField = typeof(AzureEmailServiceWrapper)
            .GetField("_emailClient", BindingFlags.NonPublic | BindingFlags.Instance);

        emailClientField?.SetValue(serviceWrapper, emailClient);
    }
}
