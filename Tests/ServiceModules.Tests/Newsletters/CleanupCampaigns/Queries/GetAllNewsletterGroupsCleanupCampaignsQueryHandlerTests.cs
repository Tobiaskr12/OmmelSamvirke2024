using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.CleanupCampaigns.Queries;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Queries;

[TestFixture, Category("UnitTests")]
public class GetAllNewsletterGroupsCleanupCampaignsQueryHandlerTests
{
    private IRepository<NewsletterGroupsCleanupCampaign> _repository;
    private GetAllNewsletterGroupsCleanupCampaignsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _handler = new GetAllNewsletterGroupsCleanupCampaignsQueryHandler(_repository);
    }

    [Test]
    public async Task Handle_ReturnsListOfCampaigns_WithExpectedProperties()
    {
        // Arrange: Create two dummy campaigns with required properties.
        DateTime now = DateTime.UtcNow;
        var testRecipient = new Recipient
        {
            EmailAddress = "Test@exmaple.com"
        };
        
        var campaign1 = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now,
            CampaignDurationMonths = 1,
            CleanedRecipients = [testRecipient],
            UncleanedRecipients = []
        };

        var campaign2 = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now.AddDays(1),
            CampaignDurationMonths = 2,
            CleanedRecipients = [],
            UncleanedRecipients = []
        };

        var campaigns = new List<NewsletterGroupsCleanupCampaign> { campaign1, campaign2 };

        _repository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaigns));

        var query = new GetAllNewsletterGroupsCleanupCampaignsQuery();

        // Act
        Result<List<NewsletterGroupsCleanupCampaign>> result = await _handler.Handle(query, default);

        // Assert: Validate that the handler returns the expected list with correct properties.
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(2));

            NewsletterGroupsCleanupCampaign? returnedCampaign1 = result.Value.FirstOrDefault(c => c.CampaignDurationMonths == 1);
            Assert.That(returnedCampaign1, Is.Not.Null);
            Assert.That(returnedCampaign1!.CampaignStart, Is.EqualTo(campaign1.CampaignStart).Within(TimeSpan.FromSeconds(1)));
            Assert.That(returnedCampaign1.CleanedRecipients, Is.Not.Empty);
            Assert.That(returnedCampaign1.CleanedRecipients.First(), Is.EqualTo(testRecipient));
            Assert.That(returnedCampaign1.UncleanedRecipients, Is.Empty);

            NewsletterGroupsCleanupCampaign? returnedCampaign2 = result.Value.FirstOrDefault(c => c.CampaignDurationMonths == 2);
            Assert.That(returnedCampaign2, Is.Not.Null);
            Assert.That(returnedCampaign2!.CampaignStart, Is.EqualTo(campaign2.CampaignStart).Within(TimeSpan.FromSeconds(1)));
            Assert.That(returnedCampaign2.CleanedRecipients, Is.Empty);
            Assert.That(returnedCampaign2.UncleanedRecipients, Is.Empty);
        });
    }
}
