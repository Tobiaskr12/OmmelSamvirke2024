using AutoFixture;
using Contracts.ServiceModules.Newsletters.CleanupCampaigns;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.CleanupCampaigns.Commands;

[TestFixture, Category("IntegrationTests")]
public class ConfirmContinuedNewsletterSubscriptionCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_RecipientNotFound_ReturnsFailure()
    {
        var command = new ConfirmContinuedNewsletterSubscriptionCommand(Guid.NewGuid());
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_ActiveCampaignNotFound_ReturnsFailure()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        await AddTestData(recipient);
        
        var command = new ConfirmContinuedNewsletterSubscriptionCommand(recipient.Token);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_ValidRecipient_MovesRecipientToCleanedAndReturnsSuccess()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        await AddTestData(recipient);
        
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        PrepareCleanupCampaign(campaign);
        campaign.UnconfirmedRecipients.Add(recipient);
        await AddTestData(campaign);

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(recipient.Token);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(campaign.UnconfirmedRecipients, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_MultipleRecipientsFoundForToken_UsesFirst()
    {
        // Arrange: Create two recipients with the same token
        var token = Guid.NewGuid();
        var recipient1 = new Recipient { Token = token, EmailAddress = "first@example.com" };
        var recipient2 = new Recipient { Token = token, EmailAddress = "second@example.com" };
        await AddTestData([recipient1, recipient2]);
        
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        PrepareCleanupCampaign(campaign);
        campaign.UnconfirmedRecipients.Add(recipient1);
        await AddTestData(campaign);

        var command = new ConfirmContinuedNewsletterSubscriptionCommand(token);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(campaign.UnconfirmedRecipients, Is.Empty);
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
