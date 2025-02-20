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
public class UnsubscribeFromNewslettersCommandHandlerTests
{
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private IRepository<Recipient> _recipientRepository;
    private IRepository<NewsletterUnsubscribeConfirmation> _unsubscribeRepository;
    private IEmailTemplateEngine _templateEngine;
    private IMediator _mediator;
    private UnsubscribeFromNewslettersCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        _unsubscribeRepository = Substitute.For<IRepository<NewsletterUnsubscribeConfirmation>>();
        _templateEngine = Substitute.For<IEmailTemplateEngine>();
        _mediator = Substitute.For<IMediator>();

        _handler = new UnsubscribeFromNewslettersCommandHandler(
            _newsletterGroupRepository,
            _recipientRepository,
            _unsubscribeRepository,
            _templateEngine,
            _mediator
        );
    }

    // Helper methods for entity creation
    private static Recipient CreateRecipient(string email) => new() { EmailAddress = email };

    private static ContactList CreateContactList(string name, string description, IEnumerable<Recipient>? recipients = null) =>
        new()
        {
            Name = name,
            Description = description,
            Contacts = recipients?.ToList() ?? []
        };

    private static NewsletterGroup CreateNewsletterGroup(int id, string name, string description, ContactList contactList) =>
        new()
        {
            Id = id,
            Name = name,
            Description = description,
            ContactList = contactList
        };

    [Test]
    public async Task Handle_NoExistingRecipient_ReturnsOk()
    {
        // Arrange
        const string email = "nosuchuser@example.com";
        var command = new UnsubscribeFromNewslettersCommand(email, [ 1 ]);

        // Create a newsletter group with a valid contact list
        NewsletterGroup group = CreateNewsletterGroup(
            1,
            "Sports Updates",
            "Latest sports news", 
            CreateContactList("Sports List", "Contact list for sports"));
        
        _newsletterGroupRepository
            .GetAllAsync(cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { group }));

        // No recipient found
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Handle_UserNotSubscribedToRequestedGroups_ReturnsFail()
    {
        // Arrange
        const string email = "user@example.com";
        var command = new UnsubscribeFromNewslettersCommand(email, [ 1, 2 ]);

        // Set up a group that doesn't include the user
        ContactList contactList = CreateContactList("News List", "General news contact list");
        NewsletterGroup group = CreateNewsletterGroup(1, "World News", "Global news updates", contactList);

        _newsletterGroupRepository
            .GetAllAsync(cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { group }));

        // We do have a recipient, but the user is not in the contact list
        Recipient existingRecipient = CreateRecipient(email);
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { existingRecipient }));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_HappyPath_CreatesUnsubscribeRequest_AndSendsEmail()
    {
        // Arrange
        const string email = "subscribed@example.com";
        var command = new UnsubscribeFromNewslettersCommand(email, [ 1 ]);

        Recipient recipient = CreateRecipient(email);
        ContactList contactList = CreateContactList("Subscribed List", "List for subscribed users", [recipient]);
        NewsletterGroup group = CreateNewsletterGroup(1, "Tech Daily", "Daily tech news", contactList);

        _newsletterGroupRepository
            .GetAllAsync(cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { group }));

        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient }));

        // No active unsubscribes
        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation>()));

        // Adding the new unsubscribe entity is successful
        _unsubscribeRepository
            .AddAsync(Arg.Any<NewsletterUnsubscribeConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<NewsletterUnsubscribeConfirmation>(default!));

        // Template engine and email sending
        _templateEngine
            .GenerateBodiesFromTemplate("UnsubscribeNewsletterConfirmationTemplate", Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Ok());

        _templateEngine.GetSubject().Returns("Bekræft din afmelding fra Ommel Samvirke");
        _templateEngine.GetHtmlBody().Returns("<html>Unsubscribe link</html>");
        _templateEngine.GetPlainTextBody().Returns("Unsubscribe link");

        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new EmailSendingStatus(
                new Email
                {
                    SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                    Subject = "Test Subject",
                    HtmlBody = "<html>Test Email</html>",
                    PlainTextBody = "Test Email",
                    Recipients = [ new Recipient { EmailAddress = email } ],
                    Attachments = []
                }, SendingStatus.Succeeded, []
            )));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess);
            await _unsubscribeRepository
                  .Received(1)
                  .AddAsync(Arg.Any<NewsletterUnsubscribeConfirmation>(), Arg.Any<CancellationToken>());
            await _mediator
                .Received(1)
                .Send(Arg.Is<SendEmailCommand>(cmd => cmd.Email.Subject.Contains("afmelding")), Arg.Any<CancellationToken>());
        });
    }

    [Test]
    public async Task Handle_ActiveUnsubscribeExists_SkipsThoseGroups()
    {
        // Arrange
        const string email = "user@example.com";
        var command = new UnsubscribeFromNewslettersCommand(email, [ 1, 2 ]);

        Recipient recipient = CreateRecipient(email);
        ContactList contactList1 = CreateContactList("List1", "Contact list 1", [recipient]);
        ContactList contactList2 = CreateContactList("List2", "Contact list 2", [recipient]);
        NewsletterGroup group1 = CreateNewsletterGroup(1, "Group1", "First group", contactList1);
        NewsletterGroup group2 = CreateNewsletterGroup(2, "Group2", "Second group", contactList2);

        _newsletterGroupRepository
            .GetAllAsync()
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { group1, group2 }));

        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient }));

        // Suppose there's an active unsubscribe for group1
        var activeUnsub = new NewsletterUnsubscribeConfirmation
        {
            IsConfirmed = false,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(1),
            Recipient = recipient,
            NewsletterGroups = [ group1 ]
        };
        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation> { activeUnsub }));

        // So group1 should be skipped, only group2 remains
        _unsubscribeRepository
            .AddAsync(Arg.Any<NewsletterUnsubscribeConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<NewsletterUnsubscribeConfirmation>(null!));

        _templateEngine
            .GenerateBodiesFromTemplate("UnsubscribeNewsletterConfirmationTemplate", Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Ok());

        _templateEngine.GetSubject().Returns("Bekræft din afmelding fra Ommel Samvirke");
        _templateEngine.GetHtmlBody().Returns("<html>Unsubscribe link</html>");
        _templateEngine.GetPlainTextBody().Returns("Unsubscribe link");

        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new EmailSendingStatus(new Email
            {
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Subject = "Test Subject",
                HtmlBody = "<html>Test Email</html>",
                PlainTextBody = "Test Email",
                Recipients = [],
                Attachments = []
            }, SendingStatus.Succeeded, [])));

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess);
            // The newly created unsubscribe should only reference group2.
            await _unsubscribeRepository
                  .Received(1)
                  .AddAsync(Arg.Is<NewsletterUnsubscribeConfirmation>(uc =>
                      uc.NewsletterGroups.Count == 1 && uc.NewsletterGroups[0].Id == 2
                  ), Arg.Any<CancellationToken>());
        });
    }
}


[TestFixture, Category("IntegrationTests")]
public class UnsubscribeFromNewslettersCommandHandlerE2ETests
{
    private IntegrationTestingHelper _integrationHelper = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _integrationHelper = new IntegrationTestingHelper();
    }

    [Test]
    public async Task HappyPath_UnsubscribeSendsConfirmationEmail()
    {
        // Arrange
        await _integrationHelper.ResetDatabase();

        // Seed a newsletter group (with a contact list containing our test email)
        var newsletterGroupRepository = _integrationHelper.GetService<IRepository<NewsletterGroup>>();
        var contactList = new ContactList
        {
            Name = "Test Contact List",
            Description = "A test contact list.",
            Contacts = [ new Recipient { EmailAddress = _integrationHelper.TestEmailClientOne.EmailAddress } ]
        };
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Newsletter Group",
            Description = "A newsletter group for testing.",
            ContactList = contactList
        };

        Result<NewsletterGroup> addGroupResult = await newsletterGroupRepository.AddAsync(newsletterGroup, CancellationToken.None);
        Assert.That(addGroupResult.IsSuccess);

        // Act: Send the unsubscribe command
        var unsubscribeCommand = new UnsubscribeFromNewslettersCommand(
            _integrationHelper.TestEmailClientOne.EmailAddress, 
            [ addGroupResult.Value.Id ]);

        Result commandResult = await _integrationHelper.Mediator.Send(unsubscribeCommand, CancellationToken.None);
        Assert.That(commandResult.IsSuccess);

        // Wait to allow the email to be sent
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Assert: Check that the unsubscribe confirmation email was received
        MimeMessage? receivedEmail = await _integrationHelper.GetLatestEmailAsync(
            _integrationHelper.TestEmailClientOne, 
            subjectIdentifier: "Bekræft din afmelding fra Ommel Samvirke"
        );

        Assert.Multiple(() =>
        {
            Assert.That(receivedEmail, Is.Not.Null);
            Assert.That(receivedEmail!.HtmlBody, Does.Contain("https://www.ommelsamvirke.com/confirm-unsubscribe?token="));
        });
    }
}
