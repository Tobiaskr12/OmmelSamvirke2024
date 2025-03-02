using AutoFixture;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetAllNewsletterGroupsCleanupCampaignsQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_ReturnsListOfCampaigns_WithExpectedProperties()
    {
        var campaign1 = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        var campaign2 = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        PrepareCleanupCampaign(campaign1);
        PrepareCleanupCampaign(campaign2);
        await AddTestData([campaign1, campaign2]);
        
        var query = new GetAllNewsletterGroupsCleanupCampaignsQuery();
        Result<List<NewsletterGroupsCleanupCampaign>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(2));

            NewsletterGroupsCleanupCampaign returnedCampaign1 = result.Value.First(x => x.Id == campaign1.Id);
            Assert.That(returnedCampaign1, Is.Not.Null);
            Assert.That(returnedCampaign1.CampaignStart, Is.EqualTo(campaign1.CampaignStart).Within(TimeSpan.FromSeconds(1)));
            Assert.That(returnedCampaign1.UnconfirmedRecipients, Is.Empty);

            NewsletterGroupsCleanupCampaign returnedCampaign2 = result.Value.First(x => x.Id == campaign2.Id);
            Assert.That(returnedCampaign2, Is.Not.Null);
            Assert.That(returnedCampaign2.CampaignStart, Is.EqualTo(campaign2.CampaignStart).Within(TimeSpan.FromSeconds(1)));
            Assert.That(returnedCampaign2.UnconfirmedRecipients, Is.Empty);
        });
    }
    
    private static void PrepareCleanupCampaign(NewsletterGroupsCleanupCampaign campaign)
    {
        campaign.UnconfirmedRecipients.Clear();
        campaign.CampaignStart = DateTime.UtcNow.AddDays(-15);
        campaign.CampaignDurationMonths = 3;
        campaign.IsCampaignStarted = true;
    }
}
