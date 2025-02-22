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

    [Test]
    public async Task SendBatchesAsync_AllBatchesSuccess_ReturnsSuccess()
    {
        // Arrange
        Email email = CreateValidEmail();
        email.Recipients =
        [
            new Recipient { EmailAddress = "recipient1@example.com" },
            new Recipient { EmailAddress = "recipient2@example.com" },
            new Recipient { EmailAddress = "recipient3@example.com" }
        ];
        const int batchSize = 2;

        var emailSendOperation = Substitute.For<EmailSendOperation>();
        emailSendOperation.HasValue.Returns(true);

        _emailClient.SendAsync(
            Arg.Any<WaitUntil>(),
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(emailSendOperation));

        var serviceWrapper = new AzureEmailServiceWrapper(_configuration, _logger);
        InjectEmailClient(serviceWrapper, _emailClient);

        // Act
        Result result = await serviceWrapper.SendBatchesAsync(email, batchSize);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            _emailClient.Received(2).SendAsync(Arg.Any<WaitUntil>(), Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        });
    }

    [Test]
    public async Task SendBatchesAsync_BatchFailure_LogsError_ButReturnsSuccess()
    {
        // Arrange
        Email email = CreateValidEmail();
        email.Recipients =
        [
            new Recipient() { EmailAddress = "recipient1@example.com" },
            new Recipient() { EmailAddress = "recipient2@example.com" },
            new Recipient() { EmailAddress = "recipient3@example.com" }
        ];
        const int batchSize = 2;

        var successOperation = Substitute.For<EmailSendOperation>();
        successOperation.HasValue.Returns(true);

        // Setup sequence: first call throws exception, second call returns success.
        _emailClient.SendAsync(
            Arg.Any<WaitUntil>(),
            Arg.Any<EmailMessage>(),
            Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException<EmailSendOperation>(new Exception("SendAsync failed")),
                Task.FromResult(successOperation)
            );

        var serviceWrapper = new AzureEmailServiceWrapper(_configuration, _logger);
        InjectEmailClient(serviceWrapper, _emailClient);

        // Act
        Result result = await serviceWrapper.SendBatchesAsync(email, batchSize);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            _logger.ReceivedWithAnyArgs().LogError(Arg.Any<Exception>());
            _emailClient.Received(2).SendAsync(Arg.Any<WaitUntil>(), Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        });
    }

    [Test]
    public async Task SendBatchesAsync_NullRecipients_ReturnsFailure()
    {
        // Arrange
        Email email = CreateValidEmail();
        email.Recipients = null!; // This will cause an exception in Chunking the recipients.
        const int batchSize = 2;

        var serviceWrapper = new AzureEmailServiceWrapper(_configuration, _logger);
        InjectEmailClient(serviceWrapper, _emailClient);

        // Act
        Result result = await serviceWrapper.SendBatchesAsync(email, batchSize);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(Errors.ErrorMessages.AzureEmailSendingFailed));
            _logger.ReceivedWithAnyArgs().LogError(Arg.Any<Exception>());
        });
    }

    private static Email CreateValidEmail()
    {
        return new Email
        {
            SenderEmailAddress = "sender@example.com",
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Subject = "Test Email",
            HtmlBody = "<h1>This is a test email</h1>",
            PlainTextBody = "<h1>This is a test email</h1>",
            Attachments = []
        };
    }

    private static void InjectEmailClient(AzureEmailServiceWrapper serviceWrapper, EmailClient emailClient)
    {
        FieldInfo? emailClientField = typeof(AzureEmailServiceWrapper)
            .GetField("_emailClient", BindingFlags.NonPublic | BindingFlags.Instance);

        emailClientField?.SetValue(serviceWrapper, emailClient);
    }
}
