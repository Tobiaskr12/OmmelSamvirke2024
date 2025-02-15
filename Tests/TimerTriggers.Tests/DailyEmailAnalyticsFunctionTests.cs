using System.Linq.Expressions;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using FluentResults;
using NSubstitute;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using TestDatabaseFixtures;

namespace TimerTriggers.Tests;

[TestFixture, Category("UnitTests")]
public class DailyEmailAnalyticsFunctionTests
{
    private IRepository<Email> _emailRepository;
    private IRepository<DailyEmailAnalytics> _dailyAnalyticsRepository;
    private DailyEmailAnalyticsFunction _function;
    private DateTime _yesterdayUtc;

    [SetUp]
    public void Setup()
    {
        _emailRepository = Substitute.For<IRepository<Email>>();
        _dailyAnalyticsRepository = Substitute.For<IRepository<DailyEmailAnalytics>>();
        
        var logger = Substitute.For<ILoggingHandler>();
        var tracer = Substitute.For<ITraceHandler>();
        _function = new DailyEmailAnalyticsFunction(logger, tracer, _emailRepository, _dailyAnalyticsRepository);
        
        _yesterdayUtc = DateTime.UtcNow.AddDays(-1);
    }
    
    [Test]
    public void Run_WhenAnalyticsSavingSucceeds_CompletesSuccessfully()
    {
        var emails = new List<Email>
        {
            CreateEmail(2, _yesterdayUtc.AddHours(-1)),
            CreateEmail(1, _yesterdayUtc.AddHours(-2))
        };
        _emailRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(emails));
    
        var analytics = new DailyEmailAnalytics
        {
            Date = _yesterdayUtc.Date,
            SentEmails = emails.Count,
            TotalRecipients = emails.Sum(email => email.Recipients.Count)
        };
        Result<DailyEmailAnalytics> successSaveResult = Result.Ok(analytics);
        _dailyAnalyticsRepository
            .AddAsync(Arg.Any<DailyEmailAnalytics>())
            .Returns(Task.FromResult(successSaveResult));
    
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrowAsync(async () => await _function.Run(null!));
            Assert.That(() => _dailyAnalyticsRepository.Received(1).AddAsync(Arg.Any<DailyEmailAnalytics>()), Throws.Nothing);
        });
    }
    
    private Email CreateEmail(int numberOfRecipients, DateTime createdTimestamp)
    {
        var recipients = new List<Recipient>();
        for (int i = 0; i < numberOfRecipients; i++)
        {
            recipients.Add(CreateRecipient());
        }
        
        return new Email
        {
            DateCreated = createdTimestamp,
            Recipients = recipients,
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Subject = "Test email",
            HtmlBody = "<h1>This is a test email</h1>",
            PlainTextBody = "This is a test email",
            Attachments = new List<Attachment>()
        };
    }
    
    private Recipient CreateRecipient()
    {
        string recipientEmailPrefix = Guid.NewGuid().ToString();
        return new Recipient
        {
            EmailAddress = recipientEmailPrefix + "@example.com"
        };
    }
}

[TestFixture, Category("IntegrationTests")]
public class DailyEmailAnalyticsFunctionIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;
    private IRepository<Email> _emailRepository;
    private IRepository<DailyEmailAnalytics> _dailyEmailAnalyticsRepository;
    private ILoggingHandler _loggingHandler;
    private ITraceHandler _traceHandler;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
        _emailRepository = _integrationTestingHelper.GetService<IRepository<Email>>();
        _dailyEmailAnalyticsRepository = _integrationTestingHelper.GetService<IRepository<DailyEmailAnalytics>>();
        _loggingHandler = _integrationTestingHelper.GetService<ILoggingHandler>();
        _traceHandler = _integrationTestingHelper.GetService<ITraceHandler>();
    }
    
    [SetUp]
    public async Task Setup()
    {
        DateTime yesterday = DateTime.UtcNow.AddDays(-1).Date;

        // Email 1: one recipient from yesterday.
        var email1 = new Email
        {
            DateCreated = yesterday.AddHours(2),
            Recipients = [new Recipient { EmailAddress = "recipient1@example.com" }],
            SenderEmailAddress = "sender@example.com",
            Subject = "Test Email 1",
            HtmlBody = "<p>Test</p>",
            PlainTextBody = "Test",
            Attachments = []
        };

        // Email 2: two recipients from yesterday.
        var email2 = new Email
        {
            DateCreated = yesterday.AddHours(5),
            Recipients =
            [
                new Recipient { EmailAddress = "recipient2@example.com" },
                new Recipient { EmailAddress = "recipient3@example.com" }
            ],
            SenderEmailAddress = "sender@example.com",
            Subject = "Test Email 2",
            HtmlBody = "<p>Test</p>",
            PlainTextBody = "Test",
            Attachments = []
        };

        // Email outside the target range (should not be counted).
        var emailOutside = new Email
        {
            DateCreated = DateTime.UtcNow.AddDays(-2),
            Recipients = [new Recipient { EmailAddress = "recipient4@example.com" }],
            SenderEmailAddress = "sender@example.com",
            Subject = "Test Email Outside",
            HtmlBody = "<p>Test</p>",
            PlainTextBody = "Test",
            Attachments = []
        };

        await _emailRepository.AddAsync(email1);
        await _emailRepository.AddAsync(email2);
        await _emailRepository.AddAsync(emailOutside);
    }

    [Test]
    public async Task DailyEmailAnalyticsFunction_Runs_CreatesAnalyticsRecord()
    {
        // Arrange
        var function = new DailyEmailAnalyticsFunction(_loggingHandler, _traceHandler, _emailRepository, _dailyEmailAnalyticsRepository);

        // Act
        await function.Run(null!);

        // Assert
        var dailyAnalyticsRepo = _integrationTestingHelper.GetService<IRepository<DailyEmailAnalytics>>();
        Result<List<DailyEmailAnalytics>> result = await dailyAnalyticsRepo.FindAsync(a => a.Date == DateTime.UtcNow.AddDays(-1).Date);
        List<DailyEmailAnalytics>? analyticsList = result.Value;
        DailyEmailAnalytics analytics = analyticsList.First();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(analyticsList, Has.Count.EqualTo(1));
            Assert.That(analytics.SentEmails, Is.EqualTo(2));
            Assert.That(analytics.TotalRecipients, Is.EqualTo(3));
        });
    }
}
