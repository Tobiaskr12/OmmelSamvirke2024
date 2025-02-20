using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.Newsletters.CleanupCampaigns.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Commands;

[TestFixture, Category("UnitTests")]
public class DeleteCleanupCampaignCommandHandlerTests
{
    private IRepository<NewsletterGroupsCleanupCampaign> _repository;
    private DeleteCleanupCampaignCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _handler = new DeleteCleanupCampaignCommandHandler(_repository);
    }

    [Test]
    public async Task Handle_CampaignNotFound_ReturnsFailure()
    {
        // Arrange: simulate GetByIdAsync failing.
        _repository
            .GetByIdAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterGroupsCleanupCampaign>());

        var command = new DeleteCleanupCampaignCommand(1);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsFailed);
            await _repository.ReceivedWithAnyArgs(1).GetByIdAsync(default!, cancellationToken: default);
            await _repository.DidNotReceiveWithAnyArgs().DeleteAsync([]);
        });
    }

    [Test]
    public async Task Handle_CampaignAlreadyStarted_ReturnsFailure()
    {
        // Arrange: create a campaign that is already started.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            IsCampaignStarted = true
        };
        _repository
            .GetByIdAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        var command = new DeleteCleanupCampaignCommand(1);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Verify failure
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsFailed);
            await _repository.ReceivedWithAnyArgs(1).GetByIdAsync(default!, cancellationToken: default);
            await _repository.DidNotReceiveWithAnyArgs().DeleteAsync([]);
        });
    }

    [Test]
    public async Task Handle_DeleteFails_ReturnsFailure()
    {
        // Arrange: create a campaign that is not started.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            IsCampaignStarted = false
        };
        _repository
            .GetByIdAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        // Simulate deletion failure.
        _repository
            .DeleteAsync(campaign, cancellationToken: default)
            .ReturnsForAnyArgs(Task.FromResult(Result.Fail("Delete failed")));

        var command = new DeleteCleanupCampaignCommand(1);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
            await _repository.ReceivedWithAnyArgs(1).GetByIdAsync(default!, cancellationToken: default);
            await _repository.ReceivedWithAnyArgs(1).DeleteAsync(campaign, cancellationToken: default);
        });
    }

    [Test]
    public async Task Handle_DeleteSucceeds_ReturnsSuccess()
    {
        // Arrange: create a campaign that is not started.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            IsCampaignStarted = false
        };
        _repository
            .GetByIdAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        // Simulate successful deletion.
        _repository
            .DeleteAsync(campaign, cancellationToken: default)
            .ReturnsForAnyArgs(Task.FromResult(Result.Ok()));

        var command = new DeleteCleanupCampaignCommand(1);

        // Act
        Result result = await _handler.Handle(command, default);

        // Assert: Verify success.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess);
            await _repository.ReceivedWithAnyArgs(1).GetByIdAsync(default!, cancellationToken: default);
            await _repository.ReceivedWithAnyArgs(1).DeleteAsync(campaign, cancellationToken: default);
        });
    }
}
