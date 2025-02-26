using AutoFixture;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Commands;

[TestFixture, Category("IntegrationTests")]
public class ConfirmUnsubscribeFromNewslettersCommandHandlerTests : ServiceTestBase
{
    private NewsletterGroup _newsletterGroup;
    
    [SetUp]
    public async Task SetUp()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);

        _newsletterGroup = newsletterGroup;
    }
    
    [Test]
    public async Task Handle_HappyPath_UnsubscribesAndDeletesOldSubscriptions()
    {
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterUnsubscribeConfirmation>();
        PrepareConfirmation(confirmation);
        await AddTestData(confirmation);
        
        var command = new ConfirmUnsubscribeFromNewslettersCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);

        Result<NewsletterUnsubscribeConfirmation> confirmationQuery =
            await GetService<IRepository<NewsletterUnsubscribeConfirmation>>().GetByIdAsync(confirmation.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(confirmationQuery.Value.IsConfirmed);
        });
    }

    [Test]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        var command = new ConfirmUnsubscribeFromNewslettersCommand(Guid.NewGuid());
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task Handle_ExpiredToken_ReturnsFailure()
    {
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterUnsubscribeConfirmation>();
        PrepareConfirmation(confirmation);
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(-1);
        await AddTestData(confirmation);
    
        var command = new ConfirmUnsubscribeFromNewslettersCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task Handle_AlreadyConfirmed_ReturnsFailure()
    {
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterUnsubscribeConfirmation>();
        PrepareConfirmation(confirmation);
        confirmation.IsConfirmed = true;
        await AddTestData(confirmation);
        
        var command = new ConfirmUnsubscribeFromNewslettersCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task Handle_CleanupCampaign_RemovesRecipientFromCampaign()
    {
        var cleanupCampaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        cleanupCampaign.IsCampaignStarted = true;
        cleanupCampaign.CampaignStart = DateTime.UtcNow.AddMonths(-1);
        cleanupCampaign.CampaignDurationMonths = 3;
        Recipient testRecipient = cleanupCampaign.UnconfirmedRecipients.First();
        await AddTestData(cleanupCampaign);
        
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterUnsubscribeConfirmation>();
        PrepareConfirmation(confirmation);
        confirmation.Recipient = testRecipient;
        await AddTestData(confirmation);
        
        var command = new ConfirmUnsubscribeFromNewslettersCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Result<NewsletterGroupsCleanupCampaign> cleanupCampaignQuery =
            await GetService<IRepository<NewsletterGroupsCleanupCampaign>>().GetByIdAsync(cleanupCampaign.Id);

        IEnumerable<int> uncleanIds = cleanupCampaignQuery.Value.UnconfirmedRecipients.Select(x => x.Id);
        IEnumerable<int> cleanIds = cleanupCampaignQuery.Value.UnconfirmedRecipients.Select(x => x.Id);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            // The recipient should be removed from both lists
            Assert.That(uncleanIds, Does.Not.Contain(testRecipient.Id));
            Assert.That(cleanIds, Does.Not.Contain(testRecipient.Id));
        });
    }
    
    private void PrepareConfirmation(NewsletterUnsubscribeConfirmation confirmation)
    {
        Recipient recipient = _newsletterGroup.ContactList.Contacts.First();
        confirmation.Recipient = recipient;
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(5);
        confirmation.NewsletterGroups = [_newsletterGroup];
        confirmation.IsConfirmed = false;
    }
}
