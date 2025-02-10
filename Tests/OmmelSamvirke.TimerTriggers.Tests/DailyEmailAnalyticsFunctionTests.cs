using System.Linq.Expressions;
using FluentResults;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using TestDatabaseFixtures;

namespace OmmelSamvirke.TimerTriggers.Tests;

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
        _function = new DailyEmailAnalyticsFunction(logger, _emailRepository, _dailyAnalyticsRepository);
        
        _yesterdayUtc = DateTime.UtcNow.AddDays(-1);
    }
    
    [Test]
    public void Run_WhenAnalyticsSavingFails_ThrowsException()
    {
        var emails = new List<Email>
        {
            CreateEmail(2, _yesterdayUtc.AddHours(-1)),
            CreateEmail(1, _yesterdayUtc.AddHours(-2))
        };
        _emailRepository.FindAsync(default!).ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(emails));
    
        Result<DailyEmailAnalytics> failedSaveResult = Result.Fail<DailyEmailAnalytics>("Unable to save analytics");
        _dailyAnalyticsRepository
            .AddAsync(Arg.Any<DailyEmailAnalytics>())
            .Returns(Task.FromResult(failedSaveResult));
    
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<Exception>(async () => await _function.Run(null!));
            Assert.That(() => _dailyAnalyticsRepository.Received(1).AddAsync(Arg.Any<DailyEmailAnalytics>()), Throws.Nothing);
        });
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
    
    [Test]
    public void Run_WhenEmailRepositoryFails_ThrowsException()
    {
        _emailRepository.FindAsync(default!).Returns(MockHelpers.FailedAsyncResult<List<Email>>());
    
        Result<DailyEmailAnalytics> failedSaveResult = Result.Fail<DailyEmailAnalytics>("Unable to save analytics");
        _dailyAnalyticsRepository
            .AddAsync(Arg.Any<DailyEmailAnalytics>())
            .Returns(Task.FromResult(failedSaveResult));
    
        Assert.Multiple(() =>
        {
            var ex = Assert.ThrowsAsync<Exception>(async () => await _function.Run(null!));
            Assert.That(ex?.Message, Does.Contain("Error retrieving number of emails sent yesterday"));
            Assert.That(() => _emailRepository.Received().FindAsync(Arg.Any<Expression<Func<Email, bool>>>()), Throws.Nothing);
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
