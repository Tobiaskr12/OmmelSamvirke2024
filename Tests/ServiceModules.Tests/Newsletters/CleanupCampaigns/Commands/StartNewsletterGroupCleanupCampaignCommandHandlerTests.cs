using AutoFixture;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Newsletters.Entities;
using FluentResults;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Commands;

[TestFixture, Category("IntegrationTests")]
public class StartNewsletterGroupCleanupCampaignCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_OverlappingCampaignExists_ReturnsFailure()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);
        
        var campaign1 = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        campaign1.CampaignStart = DateTime.UtcNow.AddMonths(1);
        campaign1.CampaignDurationMonths = 3;
        await AddTestData(campaign1);
        
        var campaign2 = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        campaign2.CampaignStart = DateTime.UtcNow.AddDays(15);
        campaign2.CampaignDurationMonths = 3;
        
        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign2);
        Result<NewsletterGroupsCleanupCampaign> result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.NewsletterGroupsCleanupCampaign_OverlappingCampaigns));
        });
    }

    [Test]
    public async Task Handle_NoNewsletterGroups_ReturnsFailure()
    {
        var campaign1 = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        campaign1.CampaignStart = DateTime.UtcNow.AddMonths(1);
        campaign1.CampaignDurationMonths = 3;
        
        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign1);
        Result<NewsletterGroupsCleanupCampaign> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert: Verify failure with specific error message.
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.NewsletterGroupsCleanupCampaign_NoNewsletterGroups));
        });
    }

    [Test]
    public async Task Handle_Success_ReturnsCampaignWithProperUncleanedRecipients()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);
        
        var campaign1 = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        campaign1.CampaignStart = DateTime.UtcNow.AddMonths(1);
        campaign1.CampaignDurationMonths = 3;
        
        var command = new StartNewsletterGroupCleanupCampaignCommand(campaign1);
        Result<NewsletterGroupsCleanupCampaign> result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.UncleanedRecipients, Has.Count.EqualTo(newsletterGroup.ContactList.Contacts.Count));
        });
    }
}
