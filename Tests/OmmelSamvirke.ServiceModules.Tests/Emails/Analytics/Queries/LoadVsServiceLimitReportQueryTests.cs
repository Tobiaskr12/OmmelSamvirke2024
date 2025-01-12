using System.Linq.Expressions;
using FluentResults;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.Analytics.Queries;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("UnitTests")]
public class LoadVsServiceLimitReportQueryTests
{
    private IRepository<Email> _emailRepository;
    private LoadVsServiceLimitReportQueryHandler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [SetUp]
    public void Setup()
    {
        _emailRepository = Substitute.For<IRepository<Email>>();
        _handler = new LoadVsServiceLimitReportQueryHandler(_emailRepository);
    }

    [Test]
    public async Task LoadVsServiceLimitReportQuery_PerMinute_ReturnsCorrectDatasets()
    {
        // Set specific startTime
        DateTime startTime = new(2025, 01, 01, 10, 00, 00, DateTimeKind.Utc);
        var query = new LoadVsServiceLimitReportQuery(startTime, ServiceLimitInterval.PerMinute);
        // For PerMinute, 12 intervals of 5 seconds each. Total period is 60 seconds.
        DateTime overallEndTime = startTime.AddSeconds(60);

        // Create emails using the helper method.
        // Buckets are defined by boundaries:
        // Bucket 1: <= startTime+5s
        // Bucket 2: <= startTime+10s
        // Bucket 3: <= startTime+15s, ... Bucket 12: <= startTime+60s.
        Email emailA = CreateTestEmail(startTime.AddSeconds(7), "a@example.com");  // falls in bucket2
        Email emailB = CreateTestEmail(startTime.AddSeconds(12), "b@example.com"); // falls in bucket3
        Email emailC = CreateTestEmail(startTime.AddSeconds(59), "c@example.com"); // falls in bucket12

        List<Email> emailList = [emailA, emailB, emailC];

        // Configure repository mock with a helper to verify the predicate logic.
        SetupRepositoryPredicate(startTime, overallEndTime, emailList);

        // Expected cumulative dataset:
        // Bucket 1 (<= 10:00:05): 0 emails.
        // Bucket 2 (<= 10:00:10): 1 email (emailA).
        // Bucket 3 (<= 10:00:15): 2 emails (emailA, emailB).
        // Buckets 4-11: still 2 emails.
        // Bucket 12 (<= 10:01:00): 3 emails (all emails).
        var expectedCumulative = new List<int>
        {
            0, // bucket1
            1, // bucket2
            2, // bucket3
            2, // bucket4
            2, // bucket5
            2, // bucket6
            2, // bucket7
            2, // bucket8
            2, // bucket9
            2, // bucket10
            2, // bucket11
            3  // bucket12
        };

        // Expected segmented dataset:
        // First element is 0. Then differences: 1-0, 2-1, then zeros, and finally 3-2.
        var expectedSegmented = new List<int>
        {
            0,  // bucket1
            1,  // bucket2: 1-0
            1,  // bucket3: 2-1
            0,  // bucket4
            0,  // bucket5
            0,  // bucket6
            0,  // bucket7
            0,  // bucket8
            0,  // bucket9
            0,  // bucket10
            0,  // bucket11
            1   // bucket12: 3-2
        };

        // Act
        Result<LoadVsServiceLimitReport> result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            LoadVsServiceLimitReport report = result.Value;
            Assert.That(report.CumulativeCounts.SequenceEqual(expectedCumulative));
            Assert.That(report.SegmentedCounts.SequenceEqual(expectedSegmented));
        });
    }

    [Test]
    public async Task LoadVsServiceLimitReportQuery_PerHour_ReturnsCorrectDatasets()
    {
        // Set specific startTime
        DateTime startTime = new(2025, 01, 01, 09, 00, 00, DateTimeKind.Utc);
        var query = new LoadVsServiceLimitReportQuery(startTime, ServiceLimitInterval.PerHour);
        // For PerHour, 12 intervals of 5 minutes each (60 minutes total).
        DateTime overallEndTime = startTime.AddMinutes(60);

        // Create emails:
        // Buckets (each 5 minutes): bucket1: <= startTime+5min, bucket2: <= startTime+10min, etc.
        // Place:
        // - emailA at startTime + 3 minutes => bucket1.
        // - emailB at startTime + 8 minutes => bucket2.
        // - emailC at startTime + 47 minutes => bucket10.
        Email emailA = CreateTestEmail(startTime.AddMinutes(3), "a@example.com");
        Email emailB = CreateTestEmail(startTime.AddMinutes(8), "b@example.com");
        Email emailC = CreateTestEmail(startTime.AddMinutes(47), "c@example.com");

        List<Email> emailList = [emailA, emailB, emailC];

        SetupRepositoryPredicate(startTime, overallEndTime, emailList);

        // Expected cumulative counts:
        // Bucket 1 (<=09:05): 1 email (emailA)
        // Bucket 2 (<=09:10): 2 emails (emailA, emailB)
        // Buckets 3-9: still 2 emails
        // Bucket 10 (<=09:50): 3 emails (emailC added)
        // Buckets 11-12: still 3 emails.
        var expectedCumulative = new List<int>
        {
            1,  // bucket1: 09:05
            2,  // bucket2: 09:10
            2,  // bucket3
            2,  // bucket4
            2,  // bucket5
            2,  // bucket6
            2,  // bucket7
            2,  // bucket8
            2,  // bucket9
            3,  // bucket10: 09:50
            3,  // bucket11
            3   // bucket12: 10:00
        };

        var expectedSegmented = new List<int>
        {
            0,  // bucket1
            1,  // bucket2: 2-1
            0,  // bucket3
            0,  // bucket4
            0,  // bucket5
            0,  // bucket6
            0,  // bucket7
            0,  // bucket8
            0,  // bucket9
            1,  // bucket10: 3-2
            0,  // bucket11
            0   // bucket12
        };

        // Act
        Result<LoadVsServiceLimitReport> result = await _handler.Handle(query, _cancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            LoadVsServiceLimitReport report = result.Value;
            Assert.That(report.CumulativeCounts.SequenceEqual(expectedCumulative));
            Assert.That(report.SegmentedCounts.SequenceEqual(expectedSegmented));
        });
    }

    [Test]
    public async Task LoadVsServiceLimitReportQuery_StartTimeInFuture_ReturnsInvalidIntervalError()
    {
        DateTime futureTime = DateTime.UtcNow.AddSeconds(10);
        var query = new LoadVsServiceLimitReportQuery(futureTime, ServiceLimitInterval.PerMinute);
        
        Result<LoadVsServiceLimitReport> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }

    [Test]
    public async Task LoadVsServiceLimitReportQuery_RepositoryFailure_ReturnsGenericError()
    {
        DateTime startTime = DateTime.UtcNow.AddMinutes(-5);
        var query = new LoadVsServiceLimitReportQuery(startTime, ServiceLimitInterval.PerMinute);

        _emailRepository
            .FindAsync(Arg.Any<Expression<Func<Email, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Fail<List<Email>>(new List<string> { "Database error" })));
        
        Result<LoadVsServiceLimitReport> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task LoadVsServiceLimitReportQuery_InvalidInterval_ReturnsInvalidIntervalError()
    {
        DateTime startTime = DateTime.UtcNow.AddMinutes(-5);
        const ServiceLimitInterval invalidInterval = (ServiceLimitInterval)99;
        var query = new LoadVsServiceLimitReportQuery(startTime, invalidInterval);
        
        Result<LoadVsServiceLimitReport> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }
    
    private static Email CreateTestEmail(DateTime dateCreated, string senderEmail)
    {
        return new Email
        {
            SenderEmailAddress = senderEmail,
            Subject = "Test Subject",
            HtmlBody = "<p>Test Body</p>",
            PlainTextBody = "Test Body",
            Recipients = [],
            Attachments = [],
            DateCreated = dateCreated
        };
    }

    // Sets up the repository mock to use a predicate that accepts emails within the 
    // [startTime, overallEndTime] window.
    private void SetupRepositoryPredicate(DateTime startTime, DateTime overallEndTime, List<Email> emailList)
    {
        _emailRepository
            .FindAsync(
                Arg.Is<Expression<Func<Email, bool>>>(expr =>
                    // Check for an email within the period.
                    expr.Compile().Invoke(CreateTestEmail(startTime.AddSeconds(30), "dummy")) &&
                    // Check for an email after overallEndTime.
                    !expr.Compile().Invoke(CreateTestEmail(overallEndTime.AddSeconds(1), "dummy"))
                ),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(Result.Ok(emailList)));
    }
}
