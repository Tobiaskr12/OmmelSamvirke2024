using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.Subscriptions.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Commands;

[TestFixture, Category("UnitTests")]
public class ConfirmNewsletterSubscriptionCommandHandlerTests
{
    private IRepository<NewsletterSubscriptionConfirmation> _subscriptionRepository;
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private ConfirmNewsletterSubscriptionCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _subscriptionRepository = Substitute.For<IRepository<NewsletterSubscriptionConfirmation>>();
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();

        _handler = new ConfirmNewsletterSubscriptionCommandHandler(
            _subscriptionRepository,
            _newsletterGroupRepository);
    }

    [Test]
    public async Task Handle_HappyPath_ReturnsSuccess()
    {
        // Arrange
        var token = Guid.NewGuid();
        var recipient = new Recipient { EmailAddress = "test@example.com" };
        var contactList = new ContactList
        {
            Id = 100,
            Name = "Test List",
            Description = "This is a test",
            Contacts = []
        };
        var group = new NewsletterGroup
        {
            Id = 200,
            Name = "Test Group",
            Description = "This is a test",
            ContactList = contactList
        };
        var confirmation = new NewsletterSubscriptionConfirmation
        {
            ConfirmationToken = token,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(1),
            IsConfirmed = false,
            Recipient = recipient,
            NewsletterGroups = [ group ]
        };

        // Subscription repository finds exactly one matching token.
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation> { confirmation }));

        // Subscription repository update is successful.
        _subscriptionRepository
            .UpdateAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(confirmation));

        // NewsletterGroup update is successful.
        _newsletterGroupRepository
            .UpdateAsync(Arg.Any<List<NewsletterGroup>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { group }));

        var command = new ConfirmNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(confirmation.IsConfirmed, Is.True);
            Assert.That(contactList.Contacts, Has.Count.EqualTo(1));
            Assert.That(contactList.Contacts.First().EmailAddress, Is.EqualTo("test@example.com"));
        });
    }

    [Test]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();

        // Subscription repository finds no matching confirmations.
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        var command = new ConfirmNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public async Task Handle_TokenExpired_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();
        var confirmation = new NewsletterSubscriptionConfirmation
        {
            ConfirmationToken = token,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(-1), // expired
            IsConfirmed = false,
            Recipient = new Recipient { EmailAddress = "test@example.com" }
        };

        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation> { confirmation }));

        var command = new ConfirmNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public async Task Handle_AlreadyConfirmed_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();
        var confirmation = new NewsletterSubscriptionConfirmation
        {
            ConfirmationToken = token,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(1),
            IsConfirmed = true, // already confirmed
            Recipient = new Recipient { EmailAddress = "test@example.com" }
        };

        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation> { confirmation }));

        var command = new ConfirmNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public async Task Handle_UpdateConfirmationFails_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();
        var confirmation = new NewsletterSubscriptionConfirmation
        {
            ConfirmationToken = token,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(1),
            IsConfirmed = false,
            Recipient = new Recipient { EmailAddress = "test@example.com" }
        };

        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation> { confirmation }));

        // Force the update call to fail.
        _subscriptionRepository
            .UpdateAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterSubscriptionConfirmation>());

        var command = new ConfirmNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public async Task Handle_UpdateGroupFails_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();
        var contactList = new ContactList { Id = 123, Contacts = [], Name = "Test", Description = "This is a test"};
        var group = new NewsletterGroup
        {
            Id = 234,
            Name = "Some group",
            ContactList = contactList,
            Description = "This is a test"
        };
        var confirmation = new NewsletterSubscriptionConfirmation
        {
            ConfirmationToken = token,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(1),
            IsConfirmed = false,
            Recipient = new Recipient { EmailAddress = "test@example.com" },
            NewsletterGroups = [ group ]
        };

        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation> { confirmation }));

        // Confirmation update succeeds...
        _subscriptionRepository
            .UpdateAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(confirmation));

        // ...but group update fails.
        _newsletterGroupRepository
            .UpdateAsync(Arg.Any<List<NewsletterGroup>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<NewsletterGroup>>());

        var command = new ConfirmNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }
}
