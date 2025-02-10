using System.Diagnostics;
using FluentResults;
using NSubstitute;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DataAccess.Emails.Repositories;
using OmmelSamvirke.DataAccess.Errors;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using TestDatabaseFixtures;

namespace OmmelSamvirke.DataAccess.Tests.Emails;

[TestFixture, Category("UnitTests")]
public class EmailSendingRepositoryTests : TestDatabaseFixture
{
    private EmailSendingRepository _emailSendingRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [SetUp]
    public void Setup()
    {
        var logger = Substitute.For<ILoggingHandler>();
        _emailSendingRepository = new EmailSendingRepository(Context, logger);
    }
    
    public static IEnumerable<TestCaseData> UsageTestCases
    {
        get
        {
            foreach (ServiceLimitInterval interval in Enum.GetValues(typeof(ServiceLimitInterval)).Cast<ServiceLimitInterval>())
            {
                int serviceLimit = interval switch
                {
                    ServiceLimitInterval.PerMinute => ServiceLimits.EmailsPerMinute,
                    ServiceLimitInterval.PerHour => ServiceLimits.EmailsPerHour,
                    _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
                };
                
                var scenarios = new List<(double currentUsagePercentage, int emailsToSend)>
                {
                    // No usage
                    (0.00, 0),
                    (0.00, serviceLimit / 2),
                    (0.00, serviceLimit),

                    // Partial usage
                    (0.50, (int)(serviceLimit * 0.5)),
                    (0.50, (int)(serviceLimit * 0.5 + serviceLimit * 0.1)),

                    // High usage
                    (0.80, (int)(serviceLimit * 0.8)),
                    (0.80, (int)(serviceLimit * 0.8 + serviceLimit * 0.1)),

                    // Full usage
                    (1.00, 0),
                    (1.00, (int)(serviceLimit * 0.1)),

                    // Exceeding usage
                    (1.20, 0),
                    (1.20, (int)(serviceLimit * 0.1))
                };

                foreach ((double currentUsagePercentage, int emailsToSend) in scenarios)
                {
                    var emailsSent = (int)Math.Round(serviceLimit * currentUsagePercentage);
                    double expectedUsagePercentage = (emailsSent + emailsToSend) / (double)serviceLimit;
                    
                    expectedUsagePercentage = Math.Round(expectedUsagePercentage, 2);

                    var testName = $"{interval}_CurrentUsage:{currentUsagePercentage * 100}%_EmailsToSend:{emailsToSend}";

                    yield return new TestCaseData(interval, currentUsagePercentage, emailsToSend, expectedUsagePercentage)
                        .SetName(testName);
                }
            }
        }
    }
    
    [Test, TestCaseSource(nameof(UsageTestCases))]
    public async Task CalculateServiceLimitAfterSendingEmails_ReturnsExpectedUsagePercentage(
        ServiceLimitInterval serviceLimitInterval,
        double currentUsagePercentage,
        int emailsToSend,
        double expectedUsagePercentage)
    {
        await InsertEmailsInDatabase(currentUsagePercentage, serviceLimitInterval);
        
        Result<double> usageResult = await _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(serviceLimitInterval, emailsToSend);
        
        if (usageResult.IsSuccess)
        {
            Assert.That(usageResult.Value, Is.EqualTo(expectedUsagePercentage).Within(0.0001),
                $"Expected usage percentage: {expectedUsagePercentage}, but got: {usageResult.Value}");
        }
        else
        {
            Assert.Fail($"Calculation failed with errors: {string.Join(", ", usageResult.Errors.Select(e => e.Message))}");
        }
    }
    
    [Test]
    public async Task CalculateServiceLimitAfterSendingEmails_NegativeEmailsToSend_ReturnsFailure()
    {
        const double currentUsage = 0.50;
        await InsertEmailsInDatabase(currentUsage, ServiceLimitInterval.PerMinute);
        const int emailsToSend = -100;
        
        Result<double> usageResult = await _emailSendingRepository.CalculateServiceLimitAfterSendingEmails(ServiceLimitInterval.PerMinute, emailsToSend);
        
        Assert.Multiple(() =>
        {
            Assert.That(usageResult.IsSuccess, Is.False, "Result should fail for negative emails to send.");
            Assert.That(usageResult.Errors, Is.Not.Empty, "There should be error messages.");
            Assert.That(usageResult.Errors.First().Message, Is.EqualTo(ErrorMessages.NumberOfEmailsToSend_Negative));
        });
    }
    
    private async Task InsertEmailsInDatabase(double targetUsagePercentage, ServiceLimitInterval interval)
    {
        int serviceLimit = interval switch
        {
            ServiceLimitInterval.PerMinute => ServiceLimits.EmailsPerMinute,
            ServiceLimitInterval.PerHour => ServiceLimits.EmailsPerHour,
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
        };

        int numberOfEmailsToInsert = interval switch
        {
            ServiceLimitInterval.PerMinute => (int)Math.Round(serviceLimit * targetUsagePercentage),
            ServiceLimitInterval.PerHour => (int)Math.Round(serviceLimit * targetUsagePercentage),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
        };

        DateTime createdDate = interval switch
        {
            ServiceLimitInterval.PerMinute => DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(40)), // Buffer for test execution time
            ServiceLimitInterval.PerHour => DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(59)), // Buffer for test execution time
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
        };

        var emails = new List<Email>();
        for (var i = 1; i <= numberOfEmailsToInsert; i++)
        {
            emails.Add(new Email
            {
                Id = i,
                Subject = "Test Email",
                HtmlBody = "This is a test email",
                PlainTextBody = "This is a test email",
                SenderEmailAddress = "test@example.com",
                Attachments = new List<Attachment>(),
                Recipients = new List<Recipient>(),
                DateCreated = createdDate,
                DateModified = createdDate
            });
        }

        await Context.Set<Email>().AddRangeAsync(emails);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    protected override Task SeedDatabase()
    {
        // Seed initial data if necessary
        return Task.CompletedTask;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Trace.Flush();
    }
}
