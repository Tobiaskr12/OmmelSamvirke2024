using AutoFixture;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using DomainModules.Newsletters.Entities;
using DomainModules.Emails.Entities;
using TimerTriggers.Emails;

namespace TimerTriggers.Tests.Emails;

[TestFixture, Category("IntegrationTests")]
public class DailyContactListAnalyticsFunctionTests : FunctionTestBase
{
    private DailyContactListAnalyticsFunction _function;
    private readonly DateTime _yesterdayUtc = DateTime.UtcNow.AddDays(-1);

    [SetUp]
    public new async Task Setup()
    {
        await base.Setup();

        var logger = GetService<ILoggingHandler>();
        var tracer = GetService<ITraceHandler>();
        var contactListRepository = GetService<IRepository<ContactList>>();
        var newsletterGroupsRepository = GetService<IRepository<NewsletterGroup>>();
        var dailyAnalyticsRepository = GetService<IRepository<DailyContactListAnalytics>>();
        _function = new DailyContactListAnalyticsFunction(logger, tracer, contactListRepository, newsletterGroupsRepository, dailyAnalyticsRepository);
    }

    [Test]
    public async Task Run_WhenEverythingSucceeds_CompletesSuccessfully()
    {
        var contactList1 = GlobalTestSetup.Fixture.Create<ContactList>();
        var contactList2 = GlobalTestSetup.Fixture.Create<ContactList>();
        contactList1.DateCreated = _yesterdayUtc;
        contactList2.DateCreated = _yesterdayUtc.AddTicks(-1);
        await AddTestData([contactList1, contactList2]);
        
        await _function.Run(null!);

        var analyticsRepository = GetService<IRepository<DailyContactListAnalytics>>();
        List<DailyContactListAnalytics>? savedAnalytics = (await analyticsRepository.GetAllAsync()).Value;

        Assert.Multiple(() =>
        {
            Assert.That(savedAnalytics, Is.Not.Null);
            Assert.That(savedAnalytics, Has.Count.EqualTo(2));
            foreach (DailyContactListAnalytics analytics in savedAnalytics)
            {
                Assert.That(analytics.Date, Is.EqualTo(_yesterdayUtc.Date));
            }
        });
    }
}
