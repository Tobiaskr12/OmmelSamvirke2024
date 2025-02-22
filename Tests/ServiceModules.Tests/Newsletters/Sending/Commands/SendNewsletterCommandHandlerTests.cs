using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Contracts.DataAccess.Emails.Enums;
using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Newsletters.Sending;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.Sending.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Sending.Commands;

[TestFixture, Category("UnitTests")]
public class SendNewsletterCommandHandlerTests
{
    private IExternalEmailServiceWrapper _emailServiceWrapper;
    private IRepository<Email> _genericEmailRepository;
    private IRepository<Recipient> _genericRecipientRepository;
    private ILoggingHandler _logger;
    private IEmailSendingRepository _emailSendingRepository;
    private IEmailTemplateEngine _emailTemplateEngine;
    private SendNewsletterCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _emailServiceWrapper = Substitute.For<IExternalEmailServiceWrapper>();
        _genericEmailRepository = Substitute.For<IRepository<Email>>();
        _genericRecipientRepository = Substitute.For<IRepository<Recipient>>();
        _logger = Substitute.For<ILoggingHandler>();
        _emailSendingRepository = Substitute.For<IEmailSendingRepository>();
        _emailTemplateEngine = Substitute.For<IEmailTemplateEngine>();

        // Simulate that calculating service usage returns 50% for both minute and hourly limits.
        _emailSendingRepository
            .CalculateServiceLimitAfterSendingEmails(
               Arg.Any<ServiceLimitInterval>(),
               Arg.Any<int>(),
               Arg.Any<CancellationToken>())
           .Returns(Task.FromResult(Result.Ok(50.0)));

        // Simulate that finding existing recipients succeeds (returning an empty list).
        _genericRecipientRepository
            .FindAsync(default)
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok(new List<Recipient>())));

        _handler = new SendNewsletterCommandHandler(
            _emailServiceWrapper,
            _genericEmailRepository,
            _genericRecipientRepository,
            _logger,
            _emailSendingRepository,
            _emailTemplateEngine
        );
    }

    [Test]
    public async Task WhenSendingNewsletter_WithValidNewsletter_ReturnsSuccessAndSendsToGroupRecipients()
    {
        // Arrange
        var recipientFromGroup = new Recipient { Id = 100, EmailAddress = "grouprecipient@example.com" };
        var contactList = new ContactList 
        { 
            Id = 1, 
            Name = "Group ContactList", 
            Description = "Desc", 
            Contacts = [recipientFromGroup]
        };
        var newsletterGroup = new NewsletterGroup 
        { 
            Id = 10, 
            Name = "Group 1", 
            Description = "Group Desc", 
            ContactList = contactList 
        };
        var email = new Email 
        { 
            Id = 50, 
            SenderEmailAddress = "sender@example.com", 
            Subject = "Newsletter Subject", 
            HtmlBody = "<p>Newsletter Content</p>", 
            PlainTextBody = "Newsletter Content", 
            Recipients = [], 
            Attachments = []
        };

        // Simulate a successful AddAsync to the email repository.
        _genericEmailRepository
            .AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Ok(email)));

        // Simulate successful email sending.
        _emailServiceWrapper
            .SendBatchesAsync(email, ServiceLimits.RecipientsPerEmail, useBcc: true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        // Act
        Result result = await _handler.Handle(new SendNewsletterCommand([newsletterGroup], email), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(email.IsNewsletter, Is.True);
        });
    }
    
    [Test]
    public async Task WhenSendingNewsletter_WithDuplicatedRecipients_RecipientsAreDeduplicated()
    {
        // Arrange
        var recipientOne = new Recipient { Id = 100, EmailAddress = "recipientone@example.com" };
        var recipientTwo = new Recipient { Id = 101, EmailAddress = "recipienttwo@example.com" };
        var contactListOne = new ContactList 
        { 
            Id = 1, 
            Name = "Group ContactList", 
            Description = "Desc", 
            Contacts = [recipientOne, recipientTwo]
        };
        var contactListTwo = new ContactList 
        { 
            Id = 2, 
            Name = "Group ContactList", 
            Description = "Desc", 
            Contacts = [recipientOne]
        };
        var newsletterGroupOne = new NewsletterGroup 
        { 
            Id = 10, 
            Name = "Group 1", 
            Description = "Group Desc", 
            ContactList = contactListOne 
        };
        var newsletterGroupTwo = new NewsletterGroup 
        { 
            Id = 11, 
            Name = "Group 1", 
            Description = "Group Desc", 
            ContactList = contactListTwo
        };
        var email = new Email 
        { 
            Id = 50, 
            SenderEmailAddress = "sender@example.com", 
            Subject = "Newsletter Subject", 
            HtmlBody = "<p>Newsletter Content</p>", 
            PlainTextBody = "Newsletter Content", 
            Recipients = [], 
            Attachments = []
        };

        // Simulate a successful AddAsync to the email repository.
        _genericEmailRepository
            .AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Ok(email)));

        // Simulate successful email sending.
        _emailServiceWrapper
            .SendBatchesAsync(email, ServiceLimits.RecipientsPerEmail, useBcc: true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        // Act
        Result result = await _handler.Handle(new SendNewsletterCommand([newsletterGroupOne, newsletterGroupTwo], email), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(email.IsNewsletter, Is.True);
            Assert.That(email.Recipients, Has.Count.EqualTo(2));
            Assert.That(email.Recipients, Is.Unique);
        });
    }

    [Test]
    public async Task WhenSendingEmailFails_ReturnsFailure()
    {
        // Arrange
        var recipientFromGroup = new Recipient { Id = 200, EmailAddress = "group2@example.com" };
        var contactList = new ContactList 
        { 
            Id = 2, 
            Name = "Another ContactList", 
            Description = "Desc", 
            Contacts = [recipientFromGroup]
        };
        var newsletterGroup = new NewsletterGroup 
        { 
            Id = 20, 
            Name = "Group 2", 
            Description = "Another Desc", 
            ContactList = contactList 
        };
        var email = new Email 
        { 
            Id = 60, 
            SenderEmailAddress = "sender@example.com", 
            Subject = "Newsletter Subject", 
            HtmlBody = "<p>Newsletter Content</p>", 
            PlainTextBody = "Newsletter Content", 
            Recipients = [], 
            Attachments = []
        };

        // Simulate a successful AddAsync to the email repository.
        _genericEmailRepository
            .AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Ok(email)));

        // Simulate email sending failure.
        _emailServiceWrapper
            .SendBatchesAsync(email, ServiceLimits.RecipientsPerEmail, useBcc: true, Arg.Any<CancellationToken>())
            .Returns(MockHelpers.FailedAsyncResult());

        // Act
        Result result = await _handler.Handle(new SendNewsletterCommand([newsletterGroup], email), CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }
}
