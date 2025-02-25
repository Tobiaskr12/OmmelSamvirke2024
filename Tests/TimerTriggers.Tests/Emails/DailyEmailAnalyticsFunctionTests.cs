using AutoFixture;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Entities;
using TimerTriggers.Emails;

namespace TimerTriggers.Tests.Emails;

[TestFixture, Category("IntegrationTests")]
public class DailyEmailAnalyticsFunctionTests : FunctionTestBase
{
    private DailyEmailAnalyticsFunction _function;
    private readonly DateTime _yesterdayUtc = DateTime.UtcNow.AddDays(-1);

     [SetUp]
     public new async Task Setup()
     {
         await base.Setup();
         
         var logger = GetService<ILoggingHandler>();
         var tracer = GetService<ITraceHandler>();
         var emailRepository = GetService<IRepository<Email>>();
         var dailyAnalyticsRepository = GetService<IRepository<DailyEmailAnalytics>>();
         _function = new DailyEmailAnalyticsFunction(logger, tracer, emailRepository, dailyAnalyticsRepository);
     }
    
    [Test]
    public async Task Run_WhenAnalyticsSavingSucceeds_CompletesSuccessfully()
    {
        var email1 = GlobalTestSetup.Fixture.Create<Email>();
        var email2 = GlobalTestSetup.Fixture.Create<Email>();
        email1.DateCreated = _yesterdayUtc;   
        email2.DateCreated = _yesterdayUtc.AddTicks(-1);
        
        // Email 3 was not sent yesterday, so it should not be included
        var email3 = GlobalTestSetup.Fixture.Create<Email>();
        email3.DateCreated = _yesterdayUtc.AddDays(-2);
        
        List<Email> emails = [email1, email2, email3];
        await AddTestData(emails);
        
        await _function.Run(null!);

        var repository = GetService<IRepository<DailyEmailAnalytics>>();
        List<DailyEmailAnalytics>? savedAnalytics = (await repository.GetAllAsync()).Value;
    
        Assert.Multiple(() =>
        {
            Assert.That(savedAnalytics, Is.Not.Null);
            Assert.That(savedAnalytics, Has.Count.EqualTo(1));
            Assert.That(savedAnalytics.First().SentEmails, Is.EqualTo(2));
        });
    }
}
