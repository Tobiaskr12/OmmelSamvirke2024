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
public class ConfirmUnsubscribeFromNewslettersCommandHandlerTests
{
    private IRepository<NewsletterUnsubscribeConfirmation> _unsubscribeRepository;
    private IRepository<NewsletterSubscriptionConfirmation> _subscriptionRepository;
    private IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private ConfirmUnsubscribeFromNewslettersCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _unsubscribeRepository = Substitute.For<IRepository<NewsletterUnsubscribeConfirmation>>();
        _subscriptionRepository = Substitute.For<IRepository<NewsletterSubscriptionConfirmation>>();
        _cleanupCampaignRepository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new ConfirmUnsubscribeFromNewslettersCommandHandler(
            _unsubscribeRepository,
            _subscriptionRepository,
            _cleanupCampaignRepository,
            _newsletterGroupRepository
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

    private static NewsletterGroup CreateNewsletterGroup(string name, string description, ContactList contactList) =>
        new()
        {
            Name = name,
            Description = description,
            ContactList = contactList
        };

    private NewsletterUnsubscribeConfirmation CreateUnsubscribeConfirmation(
        Guid token,
        Recipient recipient,
        List<NewsletterGroup> groups,
        DateTime? expiry = null,
        bool isConfirmed = false) =>
        new()
        {
            ConfirmationToken = token,
            ConfirmationExpiry = expiry ?? DateTime.UtcNow.AddDays(1),
            IsConfirmed = isConfirmed,
            Recipient = recipient,
            NewsletterGroups = groups
        };

    private static NewsletterSubscriptionConfirmation CreateSubscriptionConfirmation(Recipient recipient, List<NewsletterGroup> groups) =>
        new()
        {
            Recipient = recipient,
            NewsletterGroups = groups
        };

    [Test]
    public async Task Handle_HappyPath_UnsubscribesAndDeletesOldSubscriptions()
    {
        // Arrange
        var token = Guid.NewGuid();
        Recipient recipient = CreateRecipient("user@example.com");
        ContactList contactList = CreateContactList("User Contact List", "Contact list for user", [recipient]);
        NewsletterGroup group = CreateNewsletterGroup("Tech News", "Latest tech updates", contactList);
        NewsletterUnsubscribeConfirmation unsubscribe = CreateUnsubscribeConfirmation(token, recipient, [group]);
        NewsletterSubscriptionConfirmation subConf = CreateSubscriptionConfirmation(recipient, [group]);

        // The unsubscribe request is found
        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation> { unsubscribe }));

        // The old subscription confirmations
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation> { subConf }));

        // Deletions succeed
        _subscriptionRepository
            .DeleteAsync(Arg.Any<NewsletterSubscriptionConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Result.Ok());

        // Unsubscribe update succeeds
        _unsubscribeRepository
            .UpdateAsync(Arg.Any<NewsletterUnsubscribeConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(unsubscribe));

        // Group update succeeds
        _newsletterGroupRepository
            .UpdateAsync(Arg.Any<List<NewsletterGroup>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<List<NewsletterGroup>>([group]));

        _cleanupCampaignRepository
            .FindAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult<List<NewsletterGroupsCleanupCampaign>>([]));

        var command = new ConfirmUnsubscribeFromNewslettersCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(unsubscribe.IsConfirmed);
            Assert.That(contactList.Contacts.Any(r => r.EmailAddress == "user@example.com"), Is.False);
        });

        // Verify subscription confirmations were deleted
        await _subscriptionRepository
            .Received(1)
            .DeleteAsync(Arg.Is<NewsletterSubscriptionConfirmation>(sc => sc == subConf), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();

        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation>()));

        var command = new ConfirmUnsubscribeFromNewslettersCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();
        Recipient recipient = CreateRecipient("user@example.com");
        ContactList contactList = CreateContactList("Expired Contact List", "Contact list for expired test", [recipient]);
        NewsletterGroup group = CreateNewsletterGroup("Expired Group", "Expired group description", contactList);
        NewsletterUnsubscribeConfirmation unsubscribe = CreateUnsubscribeConfirmation(token, recipient, [group], DateTime.UtcNow.AddDays(-1));

        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation> { unsubscribe }));

        var command = new ConfirmUnsubscribeFromNewslettersCommand(token);

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
        Recipient recipient = CreateRecipient("user@example.com");
        ContactList contactList = CreateContactList("Confirmed Contact List", "Contact list for confirmed test", [recipient]);
        NewsletterGroup group = CreateNewsletterGroup("Confirmed Group", "Group already confirmed", contactList);
        NewsletterUnsubscribeConfirmation unsubscribe = CreateUnsubscribeConfirmation(token, recipient, [group], isConfirmed: true);

        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation> { unsubscribe }));

        var command = new ConfirmUnsubscribeFromNewslettersCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_UpdateUnsubscribeFails_ReturnsFailure()
    {
        // Arrange
        var token = Guid.NewGuid();
        Recipient recipient = CreateRecipient("user@example.com");
        ContactList contactList = CreateContactList("Fail Contact List", "Contact list for failure test", [recipient]);
        NewsletterGroup group = CreateNewsletterGroup("Fail Group", "Group that fails", contactList);
        NewsletterUnsubscribeConfirmation unsubscribe = CreateUnsubscribeConfirmation(token, recipient, [group]);

        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation> { unsubscribe }));
        
        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        // Force update to fail
        _unsubscribeRepository
            .UpdateAsync(unsubscribe, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterUnsubscribeConfirmation>());

        var command = new ConfirmUnsubscribeFromNewslettersCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task Handle_CleanupCampaign_RemovesRecipientFromCampaign()
    {
        // Arrange
        var token = Guid.NewGuid();
        Recipient recipient = CreateRecipient("user@example.com");
        ContactList contactList = CreateContactList("User Contact List", "Contact list for user", [recipient]);
        NewsletterGroup group = CreateNewsletterGroup("Tech News", "Latest tech updates", contactList);
        NewsletterUnsubscribeConfirmation unsubscribe = CreateUnsubscribeConfirmation(token, recipient, [group]);

        var cleanupCampaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-10),
            CampaignDurationMonths = 3,
            IsCampaignStarted = true,
            UncleanedRecipients = [recipient]
        };

        _unsubscribeRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterUnsubscribeConfirmation> { unsubscribe }));

        _subscriptionRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterSubscriptionConfirmation>()));

        _unsubscribeRepository
            .UpdateAsync(Arg.Any<NewsletterUnsubscribeConfirmation>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(unsubscribe));

        _newsletterGroupRepository
            .UpdateAsync(Arg.Any<List<NewsletterGroup>>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { group }));

        _cleanupCampaignRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { cleanupCampaign }));

        _cleanupCampaignRepository
            .UpdateAsync(Arg.Any<NewsletterGroupsCleanupCampaign>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(cleanupCampaign));

        var command = new ConfirmUnsubscribeFromNewslettersCommand(token);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(unsubscribe.IsConfirmed, Is.True);
            Assert.That(contactList.Contacts.Any(r => r.EmailAddress == recipient.EmailAddress), Is.False);
            Assert.That(cleanupCampaign.UncleanedRecipients.Any(r => r.EmailAddress == recipient.EmailAddress), Is.False);
            await _cleanupCampaignRepository.Received(1).UpdateAsync(
                Arg.Is<NewsletterGroupsCleanupCampaign>(c =>
                    c.UncleanedRecipients.All(r => !r.EmailAddress.Equals(recipient.EmailAddress, StringComparison.OrdinalIgnoreCase))
                ),
                Arg.Any<CancellationToken>());
        });
    }
}
