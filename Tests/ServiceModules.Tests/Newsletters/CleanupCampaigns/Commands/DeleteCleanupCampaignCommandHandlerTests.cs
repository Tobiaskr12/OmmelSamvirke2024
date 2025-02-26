using AutoFixture;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteCleanupCampaignCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_CampaignNotFound_ReturnsFailure()
    {
        var command = new DeleteCleanupCampaignCommand(1);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_CampaignAlreadyStarted_ReturnsFailure()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        PrepareCleanupCampaign(campaign);
        campaign.IsCampaignStarted = true;
        campaign.CampaignStart = DateTime.UtcNow.AddDays(-15);
        campaign.CampaignDurationMonths = 3;
        await AddTestData(campaign);

        var command = new DeleteCleanupCampaignCommand(1);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_DeleteSucceeds_ReturnsSuccess()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        PrepareCleanupCampaign(campaign);
        await AddTestData(campaign);

        var command = new DeleteCleanupCampaignCommand(1);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsSuccess);
    }
    
    private static void PrepareCleanupCampaign(NewsletterGroupsCleanupCampaign campaign)
    {
        campaign.UnconfirmedRecipients.Clear();
        campaign.CampaignStart = DateTime.UtcNow.AddDays(15);
        campaign.CampaignDurationMonths = 3;
        campaign.IsCampaignStarted = false;
    }
}
