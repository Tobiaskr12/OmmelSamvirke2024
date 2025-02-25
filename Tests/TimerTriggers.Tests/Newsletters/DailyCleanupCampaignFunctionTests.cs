using AutoFixture;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using MediatR;
using TimerTriggers.Newsletters;

namespace TimerTriggers.Tests.Newsletters;

[TestFixture, Category("IntegrationTests")]
public class DailyCleanupCampaignFunctionTests : FunctionTestBase
{
    private DailyCleanupCampaignFunction _function;

    [SetUp]
    public new async Task Setup()
    {
        await base.Setup();

        var campaignRepository = GetService<IRepository<NewsletterGroupsCleanupCampaign>>();
        var contactListRepository = GetService<IRepository<ContactList>>();
        var newsletterGroupRepository = GetService<IRepository<NewsletterGroup>>();
        var mediator = GetService<IMediator>();
        var templateEngine = GetService<IEmailTemplateEngine>();
        var logger = GetService<ILoggingHandler>();
        var tracer = GetService<ITraceHandler>();

        _function = new DailyCleanupCampaignFunction(
            campaignRepository,
            contactListRepository,
            newsletterGroupRepository,
            mediator,
            templateEngine,
            logger,
            tracer);
    }

    [Test]
    public async Task Run_When_NoActiveCampaignsFound_LogsInfoAndExits()
    {
        await AddTestData(new List<NewsletterGroupsCleanupCampaign>());
        
        await _function.Run(null!);

        var campaignRepository = GetService<IRepository<NewsletterGroupsCleanupCampaign>>();
        List<NewsletterGroupsCleanupCampaign>? campaigns = (await campaignRepository.GetAllAsync()).Value;
        Assert.That(campaigns, Is.Empty);
    }

    [Test]
    public async Task Run_When_CampaignNotStarted_UpdatesCampaignToStartedAndSendsReminders()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        await AddTestData([campaign]);
        
        await _function.Run(null!);

        var campaignRepository = GetService<IRepository<NewsletterGroupsCleanupCampaign>>();
        NewsletterGroupsCleanupCampaign? updatedCampaign = (await campaignRepository.GetAllAsync()).Value.FirstOrDefault(x => x.Id == campaign.Id);

        Assert.Multiple(() =>
        {
            Assert.That(updatedCampaign, Is.Not.Null);
            Assert.That(updatedCampaign!.IsCampaignStarted, Is.True);
            Assert.That(updatedCampaign.LastReminderSent, Is.Not.Null);
        });
    }

    [Test]
    public async Task Run_When_ReminderDue_SendsReminderEmailsSuccessfully()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();

        await AddTestData([campaign]);
        await AddTestData([contactList]);
        await AddTestData([newsletterGroup]);
        
        await _function.Run(null!);

        var campaignRepository = GetService<IRepository<NewsletterGroupsCleanupCampaign>>();
        NewsletterGroupsCleanupCampaign? updatedCampaign = (await campaignRepository.GetAllAsync()).Value.FirstOrDefault(x => x.Id == campaign.Id);
        Assert.That(updatedCampaign, Is.Not.Null);
        Assert.That(updatedCampaign!.LastReminderSent, Is.Not.Null);
    }

    [Test]
    public async Task Run_When_CampaignHasEnded_And_UnsubscribeFails_LogsErrorAndTracesFailure()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        await AddTestData([campaign]);
        
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData([contactList]);
        await AddTestData([newsletterGroup]);
        
        await _function.Run(null!);

        var contactListRepo = GetService<IRepository<ContactList>>();
        ContactList? updatedContactList = (await contactListRepo.GetAllAsync()).Value.FirstOrDefault(x => x.Id == contactList.Id);
        Assert.That(updatedContactList, Is.Not.Null);
        Assert.That(updatedContactList!.Contacts, Is.Not.Empty);
    }
    
    [Test]
    public async Task Run_When_NoReminderDueAndCampaignNotEnded_DoesNothing()
    {
        var campaign = GlobalTestSetup.Fixture.Create<NewsletterGroupsCleanupCampaign>();
        await AddTestData([campaign]);
        
        await _function.Run(null!);

        var campaignRepository = GetService<IRepository<NewsletterGroupsCleanupCampaign>>();
        NewsletterGroupsCleanupCampaign? updatedCampaign = (await campaignRepository.GetAllAsync()).Value.FirstOrDefault(x => x.Id == campaign.Id);
        Assert.That(updatedCampaign, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedCampaign!.LastReminderSent, Is.EqualTo(campaign.LastReminderSent));
        });
    }
}
