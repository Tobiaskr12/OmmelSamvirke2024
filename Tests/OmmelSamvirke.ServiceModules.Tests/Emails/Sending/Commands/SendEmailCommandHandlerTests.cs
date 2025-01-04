using System.Linq.Expressions;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Emails;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Sending.Commands;

[TestFixture, Category("UnitTests")]
public class SendEmailCommandHandlerTests
{
    private ILogger<SendEmailCommandHandler> _logger;
    private IRepository<Email> _genericEmailRepository;
    private IRepository<Recipient> _genericRecipientRepository;
    private IEmailSendingRepository _emailSendingRepository;
    private IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private IConfigurationRoot _configuration;
    private SendEmailCommandHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<SendEmailCommandHandler>>();
        _genericEmailRepository = Substitute.For<IRepository<Email>>();
        _genericRecipientRepository = Substitute.For<IRepository<Recipient>>();
        _emailSendingRepository = Substitute.For<IEmailSendingRepository>();
        _configuration = Substitute.For<IConfigurationRoot>();
        _externalEmailServiceWrapper = Substitute.For<IExternalEmailServiceWrapper>();

        _handler = new SendEmailCommandHandler(
            _logger,
            _genericEmailRepository,
            _genericRecipientRepository,
            _emailSendingRepository,
            _configuration,
            _externalEmailServiceWrapper);

        _genericRecipientRepository
            .FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<Recipient>());
        
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Prod");
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(50);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(50);
    }
    
    [Test]
    public async Task SendEmailCommand_ValidInput_ReturnsSuccess()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };

        var command = new SendEmailCommand(email);

        _genericEmailRepository.AddAsync(email).Returns(email);
        _externalEmailServiceWrapper.SendAsync(email).Returns(Result.Ok());

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task SendEmailCommand_AddingEmailToDbFails_ReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);

        // Simulate database error when adding email
        _genericEmailRepository.AddAsync(email).ThrowsAsync(_ => throw new Exception("Database error"));
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
    
    [TestCase(0.0)]
    [TestCase(79.99)]
    [TestCase(80.0)]
    [TestCase(80.1)]
    [TestCase(100.0)]
    public async Task SendEmailCommand_ServiceLimitIs80PercentUsed_PerHour_SendEmailToDev(double percentageUsed)
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);
        _genericEmailRepository.AddAsync(email).Returns(email);

        // Simulate service limit for PerHour interval
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(percentageUsed);
        // Ensure PerMinute is not affecting this test
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(0.0);
        
        await _handler.Handle(command, CancellationToken.None);
        
        if (percentageUsed >= 80.0)
        {
            await _externalEmailServiceWrapper.Received(1).SendAsync(
                Arg.Is<Email>(e =>
                    e.Recipients.Count == 1 &&
                    e.Recipients.Any(r => r.EmailAddress == "tobiaskristensen12@gmail.com")));
        }
        else
        {
            await _externalEmailServiceWrapper.DidNotReceive().SendAsync(
                Arg.Is<Email>(e =>
                    e.Recipients.Count == 1 &&
                    e.Recipients.Any(r => r.EmailAddress == "tobiaskristensen12@gmail.com")));
        }
    }
    
    [TestCase(0.0)]
    [TestCase(79.99)]
    [TestCase(80.0)]
    [TestCase(80.1)]
    [TestCase(100.0)]
    public async Task SendEmailCommand_ServiceLimitIs80PercentUsed_PerMinute_SendEmailToDev(double percentageUsed)
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);
        _genericEmailRepository.AddAsync(email).Returns(email);

        // Simulate service limit for PerMinute interval
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(percentageUsed);
        // Ensure PerHour is not affecting this test
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(0.0);
        
        await _handler.Handle(command, CancellationToken.None);
        
        if (percentageUsed >= 80.0)
        {
            await _externalEmailServiceWrapper.Received(1).SendAsync(
                Arg.Is<Email>(e =>
                    e.Recipients.Count == 1 &&
                    e.Recipients.Any(r => r.EmailAddress == "tobiaskristensen12@gmail.com")));
        }
        else
        {
            await _externalEmailServiceWrapper.DidNotReceive().SendAsync(
                Arg.Is<Email>(e =>
                    e.Recipients.Count == 1 &&
                    e.Recipients.Any(r => r.EmailAddress == "tobiaskristensen12@gmail.com")));
        }
    }
    
    [Test]
    public async Task SendEmailCommand_SendingEmailExceedsHourlyServiceLimit_ReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);
        _genericEmailRepository.AddAsync(email).Returns(email);

        // Simulate service limit exceeded for PerHour
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(101);
        // Ensure PerMinute does not affect this test
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(0);
        
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task SendEmailCommand_SendingEmailExceedsMinuteServiceLimit_ReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);
        _genericEmailRepository.AddAsync(email).Returns(email);

        // Simulate service limit exceeded for PerMinute
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(101);
        // Ensure PerHour does not affect this test
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(0);
        
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task SendEmailCommand_NonProdEnvironmentWithMissingWhitelist_ThrowsExceptionAndReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "notwhitelisted@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);

        // Setup non-prod environment and missing whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns((string)null!);

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SendEmailCommand_NonProdEnvironmentWithUnwhitelistedRecipient_ThrowsExceptionAndReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "notwhitelisted@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);

        // Setup non-prod environment and whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("whitelisted@example.com;another@example.com");

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task SendEmailCommand_NonProdEnvironmentWithEmptyWhitelist_ThrowsExceptionAndReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);

        // Setup non-prod environment with empty whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("");

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SendEmailCommand_NonProdEnvironmentWithPartialWhitelistedRecipients_ReturnsFail()
    {
        var email = new Email
        {
            Subject = "Partial Whitelist Test",
            HtmlBody = "Email with partial whitelisted recipients.",
            PlainTextBody = "Email with partial whitelisted recipients.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [
                new Recipient { EmailAddress = "whitelisted@example.com" },
                new Recipient { EmailAddress = "notwhitelisted@example.com" }
            ],
            Attachments = []
        };
        var command = new SendEmailCommand(email);

        // Setup non-prod environment with partial whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("whitelisted@example.com");

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SendEmailCommand_NonProdEnvironmentWithAllWhitelistedRecipients_ReturnsSuccess()
    {
        var email = new Email
        {
            Subject = "All Whitelisted Test",
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [
                new Recipient { EmailAddress = "whitelisted1@example.com" },
                new Recipient { EmailAddress = "whitelisted2@example.com" }
            ],
            Attachments = []
        };
        var command = new SendEmailCommand(email);

        // Setup non-prod environment with all recipients whitelisted
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("whitelisted1@example.com;whitelisted2@example.com");

        _genericEmailRepository.AddAsync(email).Returns(email);
        _externalEmailServiceWrapper.SendAsync(email).Returns(Result.Ok());

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess);
    }
}
