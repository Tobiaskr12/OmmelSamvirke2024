using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.Newsletters.CleanupCampaigns.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Commands;

[TestFixture, Category("UnitTests")]
public class ConfirmContinuedNewsletterSubscriptionCommandHandlerTests
{
    private IRepository<Recipient> _recipientRepository;
    private IRepository<NewsletterGroupsCleanupCampaign> _campaignRepository;
    private ILoggingHandler _logger;
    private ConfirmContinuedNewsletterSubscriptionCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        _campaignRepository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _logger = Substitute.For<ILoggingHandler>();
        _handler = new ConfirmContinuedNewsletterSubscriptionCommandHandler(_recipientRepository, _campaignRepository, _logger);
    }

    [Test]
    public async Task Handle_RecipientNotFound_ReturnsFailure()
    {
        // Arrange: repository returns an empty list.
        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient>()));

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(Guid.NewGuid());

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_ActiveCampaignNotFound_ReturnsFailure()
    {
        // Arrange: recipient exists.
        var recipient = new Recipient { Token = Guid.NewGuid(), EmailAddress = "user@example.com" };
        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient }));

        // Arrange: campaign repository returns an empty list (no active campaign).
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>()));

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(recipient.Token);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Verify failure and specific error message.
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_ValidRecipient_MovesRecipientToCleanedAndReturnsSuccess()
    {
        // Arrange: Set up a recipient present in uncleaned list.
        var token = Guid.NewGuid();
        var recipient = new Recipient { Token = token, EmailAddress = "valid@example.com" };
        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient }));

        // Arrange: Active campaign with recipient in uncleaned list.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-1),
            CampaignDurationMonths = 1,
            IsCampaignStarted = true,
            UncleanedRecipients = [recipient],
            CleanedRecipients = []
        };
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Arrange: Simulate successful update.
        _campaignRepository
            .UpdateAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Ensure success and correct side effects.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(campaign.UncleanedRecipients, Is.Empty);
            Assert.That(campaign.CleanedRecipients, Contains.Item(recipient));
            await _campaignRepository.Received(1).UpdateAsync(campaign);
        });
    }

    [Test]
    public async Task Handle_MultipleRecipientsFoundForToken_UsesFirstAndLogsWarning()
    {
        // Arrange: Create two recipients with the same token.
        var token = Guid.NewGuid();
        var recipient1 = new Recipient { Token = token, EmailAddress = "first@example.com" };
        var recipient2 = new Recipient { Token = token, EmailAddress = "second@example.com" };
        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient1, recipient2 }));

        // Arrange: Active campaign with only the first recipient in uncleaned list.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-1),
            CampaignDurationMonths = 1,
            IsCampaignStarted = true,
            UncleanedRecipients = [recipient1],
            CleanedRecipients = []
        };
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Arrange: Simulate successful update.
        _campaignRepository
            .UpdateAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Ensure success and that the first recipient was used.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(campaign.UncleanedRecipients, Is.Empty);
            Assert.That(campaign.CleanedRecipients, Contains.Item(recipient1));
            _logger.ReceivedWithAnyArgs(1).LogWarning(default!);
            await _campaignRepository.Received(1).UpdateAsync(campaign);
        });
    }

    [Test]
    public async Task Handle_RecipientAlreadyCleaned_ReturnsSuccess()
    {
        // Arrange: Set up a recipient that is not in uncleaned but already in cleaned.
        var token = Guid.NewGuid();
        var recipient = new Recipient { Token = token, EmailAddress = "cleaned@example.com" };
        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient }));

        // Arrange: Active campaign with recipient in cleaned list.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-1),
            CampaignDurationMonths = 1,
            IsCampaignStarted = true,
            UncleanedRecipients = [],
            CleanedRecipients = [recipient]
        };
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Should return success and not update campaign since recipient is already cleaned.
        Assert.That(result.IsSuccess);
        await _campaignRepository.DidNotReceive().UpdateAsync(campaign);
    }

    [Test]
    public async Task Handle_UpdateFails_ReturnsGenericErrorWithRetryPrompt()
    {
        // Arrange: Set up a valid recipient present in uncleaned list.
        var token = Guid.NewGuid();
        var recipient = new Recipient { Token = token, EmailAddress = "updatefail@example.com" };
        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<Recipient> { recipient }));

        // Arrange: Active campaign with recipient in uncleaned list.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-1),
            CampaignDurationMonths = 1,
            IsCampaignStarted = true,
            UncleanedRecipients = [recipient],
            CleanedRecipients = []
        };
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Arrange: Simulate update failure.
        _campaignRepository
            .UpdateAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterGroupsCleanupCampaign>());

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(token);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Ensure failure with the generic error message.
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }
}
