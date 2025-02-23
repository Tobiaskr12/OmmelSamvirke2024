using AutoFixture;
using Contracts.DataAccess.Emails.Enums;
using Contracts.ServiceModules.Emails.Analytics;
using Contracts.ServiceModules.Emails.Analytics.Models;
using FluentResults;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("IntegrationTests")]
public class LoadVsServiceLimitReportQueryTests : ServiceTestBase
{
    [Test]
    public async Task LoadVsServiceLimitReportQuery_PerMinute_ReturnsCorrectDatasets()
    {
        DateTime startTime = new(2025, 01, 01, 10, 00, 00, DateTimeKind.Utc);

        // Create emails using the helper method.
        // Buckets are defined by boundaries:
        // Bucket 1: <= startTime+5s
        // Bucket 2: <= startTime+10s
        // Bucket 3: <= startTime+15s, ... Bucket 12: <= startTime+60s.
        Email emailA = CreateTestEmail(startTime.AddSeconds(7));  // falls in bucket2
        Email emailB = CreateTestEmail(startTime.AddSeconds(12)); // falls in bucket3
        Email emailC = CreateTestEmail(startTime.AddSeconds(59)); // falls in bucket12
        List<Email> emailList = [emailA, emailB, emailC];

        await AddTestData(emailList);

        // Expected cumulative dataset:
        // Bucket 1 (<= 10:00:05): 0 emails.
        // Bucket 2 (<= 10:00:10): 1 email (emailA).
        // Bucket 3 (<= 10:00:15): 2 emails (emailA, emailB).
        // Buckets 4-11: still 2 emails.
        // Bucket 12 (<= 10:01:00): 3 emails (all emails).
        List<int> expectedCumulative =
        [
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
            3
        ];

        // Expected segmented dataset:
        // First element is 0. Then differences: 1-0, 2-1, then zeros, and finally 3-2.
        List<int> expectedSegmented =
        [
            0, // bucket1
            1, // bucket2: 1-0
            1, // bucket3: 2-1
            0, // bucket4
            0, // bucket5
            0, // bucket6
            0, // bucket7
            0, // bucket8
            0, // bucket9
            0, // bucket10
            0, // bucket11
            1
        ];

        // Act
        var query = new LoadVsServiceLimitReportQuery(startTime, ServiceLimitInterval.PerMinute);
        Result<LoadVsServiceLimitReport> result = await GlobalTestSetup.Mediator.Send(query);

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
        DateTime startTime = new(2025, 01, 01, 09, 00, 00, DateTimeKind.Utc);

        // Create emails:
        // Buckets (each 5 minutes): bucket1: <= startTime+5min, bucket2: <= startTime+10min, etc.
        // Place:
        // - emailA at startTime + 3 minutes => bucket1.
        // - emailB at startTime + 8 minutes => bucket2.
        // - emailC at startTime + 47 minutes => bucket10.
        Email emailA = CreateTestEmail(startTime.AddMinutes(3));
        Email emailB = CreateTestEmail(startTime.AddMinutes(8));
        Email emailC = CreateTestEmail(startTime.AddMinutes(47));
        List<Email> emailList = [emailA, emailB, emailC];

        await AddTestData(emailList);

        // Expected cumulative counts:
        // Bucket 1 (<=09:05): 1 email (emailA)
        // Bucket 2 (<=09:10): 2 emails (emailA, emailB)
        // Buckets 3-9: still 2 emails
        // Bucket 10 (<=09:50): 3 emails (emailC added)
        // Buckets 11-12: still 3 emails.
        List<int> expectedCumulative =
        [
            1, // bucket1: 09:05
            2, // bucket2: 09:10
            2, // bucket3
            2, // bucket4
            2, // bucket5
            2, // bucket6
            2, // bucket7
            2, // bucket8
            2, // bucket9
            3, // bucket10: 09:50
            3, // bucket11
            3
        ];

        List<int> expectedSegmented =
        [
            0, // bucket1
            1, // bucket2: 2-1
            0, // bucket3
            0, // bucket4
            0, // bucket5
            0, // bucket6
            0, // bucket7
            0, // bucket8
            0, // bucket9
            1, // bucket10: 3-2
            0, // bucket11
            0
        ];

        // Act
        var query = new LoadVsServiceLimitReportQuery(startTime, ServiceLimitInterval.PerHour);
        Result<LoadVsServiceLimitReport> result = await GlobalTestSetup.Mediator.Send(query);

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
        Result<LoadVsServiceLimitReport> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }

    [Test]
    public async Task LoadVsServiceLimitReportQuery_InvalidInterval_ReturnsInvalidIntervalError()
    {
        DateTime startTime = DateTime.UtcNow.AddMinutes(-5);
        const ServiceLimitInterval invalidInterval = (ServiceLimitInterval)99;
        
        var query = new LoadVsServiceLimitReportQuery(startTime, invalidInterval);
        Result<LoadVsServiceLimitReport> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }
    
    private static Email CreateTestEmail(DateTime dateCreated)
    {
        var email = GlobalTestSetup.Fixture.Create<Email>();
        email.DateCreated = dateCreated;

        return email;
    }
}
