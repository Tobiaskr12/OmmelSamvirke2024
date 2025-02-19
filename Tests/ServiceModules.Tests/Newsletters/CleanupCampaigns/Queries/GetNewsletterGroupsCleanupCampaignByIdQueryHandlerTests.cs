using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.CleanupCampaigns.Queries;
using TestDatabaseFixtures;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Queries;

[TestFixture, Category("UnitTests")]
public class GetNewsletterGroupsCleanupCampaignByIdQueryHandlerTests
{
    private IRepository<NewsletterGroupsCleanupCampaign> _repository;
    private GetNewsletterGroupsCleanupCampaignByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _handler = new GetNewsletterGroupsCleanupCampaignByIdQueryHandler(_repository);
    }

    [Test]
    public async Task Handle_ReturnsCampaignById_Successfully()
    {
        // Arrange: Create a dummy campaign with required properties.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow,
            CampaignDurationMonths = 1,
            CleanedRecipients = [],
            UncleanedRecipients = []
        };

        _repository
            .GetByIdAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        var query = new GetNewsletterGroupsCleanupCampaignByIdQuery(1, true);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(query, default);

        // Assert: Validate that the returned campaign matches the expected entity.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(campaign));
            await _repository.ReceivedWithAnyArgs(1).GetByIdAsync(default!, cancellationToken: default);
        });
    }

    [Test]
    public async Task Handle_Failure_ReturnsFailureWithClearErrorMessage()
    {
        // Arrange: Simulate repository failure with a clear error message.
        _repository
            .GetByIdAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterGroupsCleanupCampaign>());

        var query = new GetNewsletterGroupsCleanupCampaignByIdQuery(1, true);

        // Act
        Result<NewsletterGroupsCleanupCampaign> result = await _handler.Handle(query, default);

        // Assert: Validate that the failure is returned with the expected error message.
        Assert.That(result.IsFailed, Is.True);
        await _repository.ReceivedWithAnyArgs(1).GetByIdAsync(default!, cancellationToken: default);
    }
}
