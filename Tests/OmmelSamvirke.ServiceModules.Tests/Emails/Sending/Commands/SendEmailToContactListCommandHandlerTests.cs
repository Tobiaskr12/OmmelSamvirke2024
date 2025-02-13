using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Contracts.DataAccess.Emails.Enums;
using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Sending.Commands;

public class SendEmailToContactListCommandHandlerTests
{
    private IRepository<Email> _genericEmailRepository;
    private IRepository<Recipient> _genericRecipientRepository;
    private IEmailSendingRepository _emailSendingRepository;
    private IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private IConfigurationRoot _configuration;
    private IEmailTemplateEngine _emailTemplateEngine;
    private SendEmailToContactListCommandHandler _handler;
    
    private readonly Email _baseValidEmail = new()
    {
        Subject = "Test Email",
        HtmlBody = "This is a test email.",
        PlainTextBody = "This is a test email.",
        SenderEmailAddress = ValidSenderEmailAddresses.Auto,
        Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
        Attachments = []
    };
    
    private readonly ContactList _baseValidContactList = new()
    {
        Name = "Test Contact List",
        Description = "This is a test contact list.",
        Contacts = [
            new Recipient { EmailAddress = "recipient@example1.com" },
            new Recipient { EmailAddress = "recipient@example2.com" },
            new Recipient { EmailAddress = "recipient@example3.com" },
            new Recipient { EmailAddress = "recipient@example4.com" },
            new Recipient { EmailAddress = "recipient@example5.com" },
        ]
    };
    
    [SetUp]
    public void Setup()
    {
        _genericEmailRepository = Substitute.For<IRepository<Email>>();
        _genericRecipientRepository = Substitute.For<IRepository<Recipient>>();
        _emailSendingRepository = Substitute.For<IEmailSendingRepository>();
        _configuration = Substitute.For<IConfigurationRoot>();
        _externalEmailServiceWrapper = Substitute.For<IExternalEmailServiceWrapper>();
        _emailTemplateEngine = Substitute.For<IEmailTemplateEngine>();
        
        _emailTemplateEngine.GenerateBodiesFromTemplate(Arg.Any<string>(), Arg.Any<(string Key, string value)[]>()).Returns(Result.Ok());
        _emailTemplateEngine.GetHtmlBody().Returns("<h1>This is a test body for an email</h1>");
        _emailTemplateEngine.GetPlainTextBody().Returns("This is a test body for an email");

        var logger = Substitute.For<ILoggingHandler>();
        _handler = new SendEmailToContactListCommandHandler(
            logger,
            _genericEmailRepository,
            _genericRecipientRepository,
            _emailSendingRepository,
            _externalEmailServiceWrapper,
            _emailTemplateEngine,
            _configuration);
        
        _genericRecipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(new List<Recipient>());
        
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Prod");
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(50);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(50);
    }
    
    [Test]
    public async Task SendEmailToContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);

        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);
        _externalEmailServiceWrapper.SendAsync(Arg.Any<Email>()).Returns(Result.Ok());

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task SendEmailToContactListCommand_AddingEmailToDbFails_ReturnsFail()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);

        // Simulate database error when adding email
        _genericEmailRepository.AddAsync(Arg.Any<Email>()).ThrowsAsync(_ => throw new Exception("Database error"));
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
    
    [TestCase(0.0)]
    [TestCase(79.99)]
    [TestCase(80.0)]
    [TestCase(80.1)]
    [TestCase(100.0)]
    public async Task SendEmailToContactListCommand_ServiceLimitIs80PercentUsed_PerHour_SendEmailToDev(double percentageUsed)
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);
        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);

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
    public async Task SendEmailToContactListCommand_ServiceLimitIs80PercentUsed_PerMinute_SendEmailToDev(double percentageUsed)
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);
        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);

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
    public async Task SendEmailToContactListCommand_SendingEmailExceedsHourlyServiceLimit_ReturnsFail()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);
        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);

        // Simulate service limit exceeded for PerHour
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(101);
        // Ensure PerMinute does not affect this test
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(0);
        
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task SendEmailToContactListCommand_SendingEmailExceedsMinuteServiceLimit_ReturnsFail()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);
        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);

        // Simulate service limit exceeded for PerMinute
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(101);
        // Ensure PerHour does not affect this test
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(0);
        
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }

    [TestCase(1, 5, 1)]
    [TestCase(5, 5, 1)]
    [TestCase(6, 5, 2)]
    [TestCase(10, 5, 2)]
    [TestCase(100, 5, 20)]
    [TestCase(101, 5, 21)]
    public async Task SendEmailToContactListCommand_HandlerCreatesTheCorrectNumberOfBatches(int contactsCount, int batchSize, int expectedNumberOfBatches)
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, CreateContactList(contactsCount), batchSize);
        
        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);
        _externalEmailServiceWrapper.SendAsync(Arg.Any<Email>()).Returns(Result.Ok());

        await _handler.Handle(command, CancellationToken.None);
        
        await _genericEmailRepository.Received(expectedNumberOfBatches).AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        await _externalEmailServiceWrapper.Received(expectedNumberOfBatches).SendAsync(Arg.Any<Email>(), cancellationToken: Arg.Any<CancellationToken>());
    }
    
    [Test]
    public async Task SendEmailToContactListCommand_NonProdEnvironmentWithMissingWhitelist_ThrowsExceptionAndReturnsFail()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);

        // Setup non-prod environment and missing whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns((string)null!);

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SendEmailToContactListCommand_NonProdEnvironmentWithUnwhitelistedRecipient_ThrowsExceptionAndReturnsFail()
    {
        var contactList = new ContactList
        {
            Name = "Test Contact List",
            Description = "This is a test contact list.",
            Contacts = [new Recipient { EmailAddress = "notwhitelisted@example.com" }]
        };
        var command = new SendEmailToContactListCommand(_baseValidEmail, contactList, 3);

        // Setup non-prod environment and whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("whitelisted@example.com;another@example.com");

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    private static ContactList CreateContactList(int contactsCount)
    {
        List<Recipient> recipients = [];
        
        for (var i = 0; i < contactsCount; i++)
        {
            recipients.Add(new Recipient { EmailAddress =  $"test{i}@example.com" });   
        }

        return new ContactList
        {
            Name = "Test Name",
            Description = "Test Description",
            Contacts = recipients
        };
    }
    
    [Test]
    public async Task SendEmailToContactListCommand_NonProdEnvironmentWithEmptyWhitelist_ThrowsExceptionAndReturnsFail()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);

        // Setup non-prod environment with empty whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("");

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SendEmailToContactListCommand_NonProdEnvironmentWithAllRecipientsWhitelisted_ReturnsSuccess()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);

        // Setup non-prod environment with recipients whitelisted
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns(string.Join(";", _baseValidContactList.Contacts.Select(c => c.EmailAddress)));

        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);
        _externalEmailServiceWrapper.SendAsync(Arg.Any<Email>()).Returns(Result.Ok());
        
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task SendEmailToContactListCommand_NonProdEnvironmentWithUnwhitelistedRecipients_ReturnsFail()
    {
        var unwhitelistedContactList = new ContactList
        {
            Name = "Test Contact List",
            Description = "List with unwhitelisted recipients.",
            Contacts = [
                new Recipient { EmailAddress = "unwhitelisted1@example.com" },
                new Recipient { EmailAddress = "unwhitelisted2@example.com" }
            ]
        };

        var command = new SendEmailToContactListCommand(_baseValidEmail, unwhitelistedContactList, 3);

        // Setup non-prod environment with limited whitelist
        _configuration.GetSection("ExecutionEnvironment").Value.Returns("Dev");
        _configuration.GetSection("EmailWhitelist").Value.Returns("whitelisted@example.com");

        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }
}
