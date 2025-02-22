using AutoFixture;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetNewsletterGroupsCleanupCampaignByIdQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_ReturnsCampaignById_Successfully()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        await AddTestData(campaign);
        
        var query = new GetNewsletterGroupsCleanupCampaignByIdQuery(campaign.Id, true);
        Result<NewsletterGroupsCleanupCampaign> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo(campaign.Id));
        });
    }
}
