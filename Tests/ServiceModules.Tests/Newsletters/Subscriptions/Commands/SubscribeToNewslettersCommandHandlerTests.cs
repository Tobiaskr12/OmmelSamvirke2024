using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Enums;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using MimeKit;
using NSubstitute;
using ServiceModules.Newsletters.Subscriptions.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Commands;

[TestFixture, Category("UnitTests")]
public class SubscribeToNewslettersCommandHandlerTests
{
    private IRepository<NewsletterSubscriptionConfirmation> _subscriptionRepository;
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private IRepository<Recipient> _recipientRepository;
    private IEmailTemplateEngine _templateEngine;
    private IMediator _mediator;
    private SubscribeToNewslettersCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _subscriptionRepository = Substitute.For<IRepository<NewsletterSubscriptionConfirmation>>();
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        _templateEngine = Substitute.For<IEmailTemplateEngine>();
        _mediator = Substitute.For<IMediator>();
        _handler = new SubscribeToNewslettersCommandHandler(
            _subscriptionRepository,
            _newsletterGroupRepository,
            _recipientRepository,
            _templateEngine,
            _mediator);
    }

    [Test]
    public async Task Handle_HappyPath_NewRecipient_ReturnsSuccess_AndSendsConfirmationEmail()
    {
        // Arrange
        const string emailAddress = "user@example.com";
        const int newsletterGroupId = 1;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository to return one valid group (recipient not subscribed).
        var contactList = new ContactList
        {
            Id = 10,
            Name = "Test List",
            Description = "Test List",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group 1",
            Description = "Description",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Setup recipient repository: no existing recipient.
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));

        // Setup subscription repository: no pending confirmations.
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        // Setup AddAsync to succeed.
        _subscriptionRepository
            .AddAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<NewsletterSubscriptionConfirmation>(default!));

        // Setup email template generation to succeed.
        _templateEngine
            .GenerateBodiesFromTemplate(default!)
            .ReturnsForAnyArgs(Result.Ok());
        _templateEngine.GetSubject().Returns("Confirm your subscription");
        _templateEngine.GetHtmlBody().Returns("<html>Confirm link</html>");
        _templateEngine.GetPlainTextBody().Returns("Confirm link");

        // Setup mediator to succeed sending email.
        var emailSendingStatus = new EmailSendingStatus(
            new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
                Subject = "Confirm your subscription",
                HtmlBody = "<html>Confirm link</html>",
                PlainTextBody = "Confirm link",
                Recipients = [new Recipient { EmailAddress = emailAddress }],
                Attachments = []
            },
            SendingStatus.Succeeded,
            []);
        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(emailSendingStatus));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Handle_HappyPath_ExistingRecipient_ReturnsSuccess_AndSendsConfirmationEmail()
    {
        // Arrange
        const string emailAddress = "existing@example.com";
        const int newsletterGroupId = 1;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository: return a valid group.
        var contactList = new ContactList
        {
            Id = 20,
            Name = "Existing List",
            Description = "Existing List",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group Existing",
            Description = "Description",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Setup recipient repository: return an existing recipient.
        var existingRecipient = new Recipient { EmailAddress = emailAddress };
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { existingRecipient }));

        // Setup subscription repository: no pending confirmations.
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        // Setup AddAsync to succeed.
        _subscriptionRepository
            .AddAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<NewsletterSubscriptionConfirmation>(null!));

        // Setup email template generation to succeed.
        _templateEngine
            .GenerateBodiesFromTemplate("NewsletterSubscriptionConfirmationTemplate", Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Ok());
        _templateEngine.GetSubject().Returns("Confirm your subscription");
        _templateEngine.GetHtmlBody().Returns("<html>Confirm link</html>");
        _templateEngine.GetPlainTextBody().Returns("Confirm link");

        // Setup mediator to succeed sending email.
        var emailSendingStatus = new EmailSendingStatus(
            new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
                Subject = "Confirm your subscription",
                HtmlBody = "<html>Confirm link</html>",
                PlainTextBody = "Confirm link",
                Recipients = [new Recipient { EmailAddress = emailAddress }],
                Attachments = []
            },
            SendingStatus.Succeeded,
            []);
        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(emailSendingStatus));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Handle_SkipsAlreadySubscribedGroups_ReturnsFailureIfAllGroupsSkipped()
    {
        // Arrange: User already subscribed to the only requested group.
        const string emailAddress = "user@example.com";
        const int newsletterGroupId = 1;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository: return a group where the recipient is already in the contact list.
        var contactList = new ContactList
        {
            Id = 30,
            Name = "Already Subscribed List",
            Description = "List Desc",
            Contacts = [new Recipient { EmailAddress = emailAddress }]
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group Subscribed",
            Description = "Desc",
            ContactList = contactList
        };

        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));
        
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert: Should fail because after filtering, no groups remain.
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_TooManyPendingConfirmations_ReturnsFailure()
    {
        // Arrange
        const string emailAddress = "user@example.com";
        const int newsletterGroupId = 2;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository: return a valid group.
        var contactList = new ContactList
        {
            Id = 40,
            Name = "List",
            Description = "Desc",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group 2",
            Description = "Desc",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Setup recipient repository: no existing recipient.
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));

        // Setup subscription repository: return more than 5 pending confirmations.
        List<NewsletterSubscriptionConfirmation> pendingList = Enumerable.Range(1, 6).Select(_ => new NewsletterSubscriptionConfirmation
        {
            Recipient = new Recipient { EmailAddress = emailAddress },
            ConfirmationExpiry = DateTime.UtcNow.AddDays(1),
            IsConfirmed = false
        }).ToList();
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(pendingList));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_AddSubscriptionConfirmationFails_ReturnsFailure()
    {
        // Arrange
        const string emailAddress = "user@example.com";
        const int newsletterGroupId = 1;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository: return a valid group.
        var contactList = new ContactList
        {
            Id = 50,
            Name = "Test List",
            Description = "Desc",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group 1",
            Description = "Desc",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Setup recipient repository: no existing recipient.
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));
        
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        // Setup subscription repository AddAsync to fail.
        _subscriptionRepository
            .AddAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterSubscriptionConfirmation>());

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_EmailTemplateGenerationFails_ReturnsFailure()
    {
        // Arrange
        const string emailAddress = "user@example.com";
        const int newsletterGroupId = 1;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository: return a valid group.
        var contactList = new ContactList
        {
            Id = 60,
            Name = "Test List",
            Description = "Desc",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group 1",
            Description = "Desc",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Setup recipient repository: no existing recipient.
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));

        // Setup subscription repository AddAsync to succeed.
        _subscriptionRepository
            .AddAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<NewsletterSubscriptionConfirmation>(default!));
        
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        // Setup email template generation to fail.
        _templateEngine
            .GenerateBodiesFromTemplate("NewsletterSubscriptionConfirmationTemplate", Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Fail("Template generation failed"));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_EmailSendingFails_ReturnsFailure()
    {
        // Arrange
        const string emailAddress = "user@example.com";
        const int newsletterGroupId = 1;
        var command = new SubscribeToNewslettersCommand(emailAddress, [newsletterGroupId]);

        // Setup newsletter group repository: return a valid group.
        var contactList = new ContactList
        {
            Id = 70,
            Name = "Test List",
            Description = "Desc",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Id = newsletterGroupId,
            Name = "Group 1",
            Description = "Desc",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Setup recipient repository: no existing recipient.
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));
        
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        // Setup subscription repository AddAsync to succeed.
        _subscriptionRepository
            .AddAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<NewsletterSubscriptionConfirmation>(default!));

        // Setup email template generation to succeed.
        _templateEngine
            .GenerateBodiesFromTemplate("NewsletterSubscriptionConfirmationTemplate", Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Ok());
        _templateEngine.GetSubject().Returns("Confirm your subscription");
        _templateEngine.GetHtmlBody().Returns("<html>Confirm link</html>");
        _templateEngine.GetPlainTextBody().Returns("Confirm link");

        // Setup mediator to fail sending email.
        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<EmailSendingStatus>());

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }
}

[TestFixture, Category("IntegrationTests")]
public class SubscribeToNewslettersCommandHandlerE2ETests : ServiceTestBase
{
    [Test]
    public async Task HappyPath_SubscribeSendsConfirmationEmail()
    {
        // Seed a newsletter group (with an empty contact list) into the database.
        var newsletterGroupRepository = GetService<IRepository<NewsletterGroup>>();
        var contactList = new ContactList
        {
            Name = "Test Contact List",
            Description = "A test contact list.",
            Contacts = []
        };
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Newsletter Group",
            Description = "A newsletter group for testing.",
            ContactList = contactList
        };

        Result<NewsletterGroup> addGroupResult = await newsletterGroupRepository.AddAsync(newsletterGroup, CancellationToken.None);
        Assert.That(addGroupResult.IsSuccess, "Failed to seed newsletter group.");

        // Create the subscribe command using the test email and the seeded newsletter group's id.
        var subscribeCommand = new SubscribeToNewslettersCommand(GlobalTestSetup.TestEmailClientOne.EmailAddress, [addGroupResult.Value.Id]);

        // Act: Send the subscribe command.
        Result commandResult = await GlobalTestSetup.Mediator.Send(subscribeCommand, CancellationToken.None);
        Assert.That(commandResult.IsSuccess);

        // Wait to allow the email to be sent.
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Assert
        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, subjectIdentifier: "Bekræft din tilmelding til Ommel Samvirkes nyhedsbrev");
        
        Assert.Multiple(() =>
        {
            
            Assert.That(receivedEmail, Is.Not.Null);
            Assert.That(receivedEmail!.HtmlBody, Does.Contain("<a href=\"https://www.ommelsamvirke.com/confirm-subscription?token="));
        });
    }
}
