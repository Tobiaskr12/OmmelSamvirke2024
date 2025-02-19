using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.Newsletters.CleanupCampaigns.Commands;
using TestDatabaseFixtures;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Commands;

[TestFixture, Category("UnitTests")]
public class StartNewsletterGroupCleanupCampaignCommandHandlerTests
{
    private IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private StartNewsletterGroupCleanupCampaignCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _cleanupCampaignRepository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new StartNewsletterGroupCleanupCampaignCommandHandler(_cleanupCampaignRepository, _newsletterGroupRepository);
    }

    [Test]
    public async Task Handle_OverlappingCampaignExists_ReturnsFailure()
    {
        // Arrange: simulate overlapping campaigns exist.
        _cleanupCampaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>
            {
                new()
                {
                    CampaignStart = default,
                    CampaignDurationMonths = 0
                }
            }));

        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            CleanedRecipients = [],
            UncleanedRecipients = []
        };
        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(command, default);

        // Assert: Verify failure with specific error message.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.NewsletterGroupsCleanupCampaign_OverlappingCampaigns));
            await _cleanupCampaignRepository.ReceivedWithAnyArgs(1).FindAsync(default!, cancellationToken: default);
        });
    }

    [Test]
    public async Task Handle_NewsletterGroupsRetrievalFails_ReturnsFailure()
    {
        // Arrange: No overlapping campaigns.
        _cleanupCampaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>()));
        // Simulate newsletter groups retrieval failure.
        _newsletterGroupRepository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<NewsletterGroup>>());

        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            CleanedRecipients = [],
            UncleanedRecipients = []
        };
        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(command, default);

        await Assert.MultipleAsync(async () =>
        {
            // Assert: Verify failure with generic error message.
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
            await _newsletterGroupRepository.ReceivedWithAnyArgs(1).GetAllAsync(default);
        });
    }

    [Test]
    public async Task Handle_NoNewsletterGroups_ReturnsFailure()
    {
        // Arrange: No overlapping campaigns.
        _cleanupCampaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>()));
        // Arrange: newsletter group retrieval returns an empty list.
        _newsletterGroupRepository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup>()));

        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            CleanedRecipients = [],
            UncleanedRecipients = []
        };
        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(command, default);

        // Assert: Verify failure with specific error message.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.NewsletterGroupsCleanupCampaign_NoNewsletterGroups));
            await _newsletterGroupRepository.ReceivedWithAnyArgs(1).GetAllAsync(default);
        });
    }

    [Test]
    public async Task Handle_SaveCampaignFails_ReturnsFailure()
    {
        // Arrange: No overlapping campaigns.
        _cleanupCampaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>()));
            
        // Arrange: newsletter groups retrieval returns at least one group.
        var contactList = new ContactList
        {
            Name = "Test Contact List",
            Description = "Test Description",
            Contacts = [new Recipient { EmailAddress = "a@example.com" }]
        };
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Newsletter Group",
            Description = "Group Description",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            CleanedRecipients = [],
            UncleanedRecipients = []
        };
        // Simulate AddAsync failure.
        _cleanupCampaignRepository
            .AddAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterGroupsCleanupCampaign>());

        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(command, default);

        // Assert: Verify failure with generic error message.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
            await _cleanupCampaignRepository.ReceivedWithAnyArgs(1).AddAsync(campaign);
        });
    }

    [Test]
    public async Task Handle_Success_ReturnsCampaignWithProperUncleanedRecipients()
    {
        // Arrange: No overlapping campaigns.
        _cleanupCampaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>()));
            
        // Arrange: newsletter groups retrieval returns a group with two contacts.
        var recipientA = new Recipient { EmailAddress = "a@example.com" };
        var recipientB = new Recipient { EmailAddress = "b@example.com" };
        var contactList = new ContactList
        {
            Name = "Test Contact List",
            Description = "Test Description",
            Contacts = [recipientA, recipientB]
        };
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Newsletter Group",
            Description = "Group Description",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Arrange: Create a campaign that already has recipientA in its CleanedRecipients.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            CleanedRecipients = [new Recipient { EmailAddress = "a@example.com" }],
            UncleanedRecipients = []
        };
            
        _cleanupCampaignRepository
            .AddAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(command, default);

        // Assert: Expect success and that UncleanedRecipients contains only recipientB.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(campaign.UncleanedRecipients, Has.Count.EqualTo(1));
            Assert.That(campaign.UncleanedRecipients.First().EmailAddress, Is.EqualTo("b@example.com"));
            await _newsletterGroupRepository.ReceivedWithAnyArgs(1).GetAllAsync(default);
            await _cleanupCampaignRepository.ReceivedWithAnyArgs(1).AddAsync(campaign);
        });
    }
}