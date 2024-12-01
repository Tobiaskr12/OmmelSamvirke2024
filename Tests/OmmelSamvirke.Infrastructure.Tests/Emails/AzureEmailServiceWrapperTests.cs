using System.Net.Mime;
using System.Reflection;
using Azure;
using Azure.Communication.Email;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Emails;

namespace OmmelSamvirke.Infrastructure.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class AzureEmailServiceWrapperTests
{
    private Mock<IConfiguration> _configurationMock;
    private Mock<ILogger<AzureEmailServiceWrapper>> _loggerMock;
    private Mock<EmailClient>? _emailClientMock;

    private const string ValidConnectionString = "Endpoint=https://example.com/;AccessKey=examplekey";

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AzureEmailServiceWrapper>>();
        _emailClientMock = new Mock<EmailClient>(ValidConnectionString);

        // Mock the ConnectionStrings section
        var connectionStringsSectionMock = new Mock<IConfigurationSection>();
        connectionStringsSectionMock.Setup(a => a["AcsConnectionString"]).Returns(ValidConnectionString);

        _configurationMock.Setup(c => c.GetSection("ConnectionStrings"))
                          .Returns(connectionStringsSectionMock.Object);
    }

    [Test]
    public async Task SendAsync_EmailClientThrowsException_ReturnsFailureResult()
    {
        Email email = CreateValidEmail();

        // Mock EmailClient.SendAsync to throw an exception
        _emailClientMock = new Mock<EmailClient>(ValidConnectionString);
        _emailClientMock.Setup(client => client.SendAsync(
            It.IsAny<WaitUntil>(),
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SendAsync failed"));
        
        var serviceWrapper = new AzureEmailServiceWrapper(_configurationMock.Object, _loggerMock.Object);
        InjectEmailClient(serviceWrapper, _emailClientMock.Object);
        Result<EmailSendingStatus> result = await serviceWrapper.SendAsync(email);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(Errors.ErrorMessages.AzureEmailSendingFailed));
        });
    }

    [Test]
    public async Task SendAsync_ReturnedSendOperationHasNoValue_ReturnsFail()
    {
        Email email = CreateValidEmail();

        // Mock EmailClient.SendAsync to return an operation with HasValue = false
        _emailClientMock = new Mock<EmailClient>(ValidConnectionString);
        var emailSendOperationMock = new Mock<EmailSendOperation>();
        emailSendOperationMock.SetupGet(op => op.HasValue).Returns(false);

        _emailClientMock.Setup(client => client.SendAsync(
            It.IsAny<WaitUntil>(),
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(emailSendOperationMock.Object);
        
        var serviceWrapper = new AzureEmailServiceWrapper(_configurationMock.Object, _loggerMock.Object);
        InjectEmailClient(serviceWrapper, _emailClientMock.Object);
        Result<EmailSendingStatus> result = await serviceWrapper.SendAsync(email);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task ConvertEmailToAzureEmailMessage_InvalidAttachment_ReturnsFail()
    {
        Email email = CreateValidEmail();
        email.Attachments.Add(new Attachment
        {
            Name = "test.txt",
            ContentType = new ContentType { Name = "text/plain" },
            ContentPath = new Uri("https://www.test.example.com"),
            BinaryContent = null // Null content is not valid
        });
        
        _emailClientMock = new Mock<EmailClient>(ValidConnectionString);
        var emailSendOperationMock = new Mock<EmailSendOperation>();
        emailSendOperationMock.SetupGet(op => op.HasValue).Returns(false);

        _emailClientMock.Setup(client => client.SendAsync(
                            It.IsAny<WaitUntil>(),
                            It.IsAny<EmailMessage>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(emailSendOperationMock.Object);

        var serviceWrapper = new AzureEmailServiceWrapper(_configurationMock.Object, _loggerMock.Object);
        InjectEmailClient(serviceWrapper, _emailClientMock.Object);
        Result<EmailSendingStatus> result = await serviceWrapper.SendAsync(email);
        
        Assert.That(result.IsFailed);
    }

    private static Email CreateValidEmail()
    {
        return new Email
        {
            SenderEmailAddress = "sender@example.com",
            Recipients = [new Recipient() { EmailAddress = "recipient@example.com" }],
            Subject = "Test Email",
            Body = "<h1>This is a test email</h1>",
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