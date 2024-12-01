using FluentResults;
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
using OmmelSamvirke.ServiceModules.Emails.Features.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class EmailSendingTests
{
    private ILogger<SendEmailCommandHandler> _logger;
    private IRepository<Email> _genericEmailRepository;
    private IEmailSendingRepository _emailSendingRepository;
    private IExternalEmailServiceWrapper _externalEmailServiceWrapper;
    private SendEmailCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<SendEmailCommandHandler>>();
        _genericEmailRepository = Substitute.For<IRepository<Email>>();
        _emailSendingRepository = Substitute.For<IEmailSendingRepository>();
        _externalEmailServiceWrapper = Substitute.For<IExternalEmailServiceWrapper>();

        _handler = new SendEmailCommandHandler(
            _logger,
            _genericEmailRepository,
            _emailSendingRepository,
            _externalEmailServiceWrapper);
    }

    // TODO - Add test for validator
    
    [Test]
    public async Task SendEmailCommand_ValidInput_ReturnsSuccess()
    {
        var email = new Email
        {
            Subject = "Test Email",
            Body = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };

        var command = new SendEmailCommand(email);

        _genericEmailRepository.AddAsync(email).Returns(email);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(50);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(50);
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
            Body = "This is a test email.",
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

    // TODO - Split test in 2 - One for minute limit and one for hourly
    [Test]
    public async Task SendEmailCommand_SendingEmailExceedsServiceLimits_ReturnsFail()
    {
        var email = new Email
        {
            Subject = "Test Email",
            Body = "This is a test email.",
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Recipients = [new Recipient { EmailAddress = "recipient@example.com" }],
            Attachments = []
        };
        var command = new SendEmailCommand(email);
        _genericEmailRepository.AddAsync(email).Returns(email);
        
        // Simulate service limits exceeded
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerHour, Arg.Any<int>()).Returns(101);
        _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, Arg.Any<int>()).Returns(101);
        
        Result<EmailSendingStatus> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
}
