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
using FluentResults;

namespace OmmelSamvirke.ServiceModules.Tests.Emails;

public class SendEmailToContactListCommandHandlerTests
{
    private ILogger<SendEmailCommandHandler> _logger;
    private IRepository<Email> _genericEmailRepository;
    private IEmailSendingRepository _emailSendingRepository;
    private IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private SendEmailToContactListCommandHandler _handler;
    
    private readonly Email _baseValidEmail = new()
    {
        Subject = "Test Email",
        Body = "This is a test email.",
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
        _logger = Substitute.For<ILogger<SendEmailCommandHandler>>();
        _genericEmailRepository = Substitute.For<IRepository<Email>>();
        _emailSendingRepository = Substitute.For<IEmailSendingRepository>();
        _externalEmailServiceWrapper = Substitute.For<IExternalEmailServiceWrapper>();

        _handler = new SendEmailToContactListCommandHandler(
            _logger,
            _genericEmailRepository,
            _emailSendingRepository,
            _externalEmailServiceWrapper);
    }
    
    [Test]
    public async Task SendEmailToContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new SendEmailToContactListCommand(_baseValidEmail, _baseValidContactList, 3);

        _genericEmailRepository.AddAsync(Arg.Any<Email>()).Returns(_baseValidEmail);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(50);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(50);
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
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(50);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(50);
        _externalEmailServiceWrapper.SendAsync(Arg.Any<Email>()).Returns(Result.Ok());

        await _handler.Handle(command, CancellationToken.None);
        
        await _genericEmailRepository.Received(expectedNumberOfBatches).AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        await _externalEmailServiceWrapper.Received(expectedNumberOfBatches).SendAsync(Arg.Any<Email>(), cancellationToken: Arg.Any<CancellationToken>());
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
}
