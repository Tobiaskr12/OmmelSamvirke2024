using AutoFixture;
using Contracts.ServiceModules.Emails.Analytics;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("IntegrationTests")]
public class DailyEmailAnalyticsQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_WhenSingleRecordExists_ReturnsRecord()
    {
        var testData = GlobalTestSetup.Fixture.Create<DailyEmailAnalytics>();
        await AddTestData(testData);
        
        var query = new DailyEmailAnalyticsQuery(testData.Date);
        Result<DailyEmailAnalytics?> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(testData.Id));
        });
    }

    [Test]
    public async Task Handle_WhenNoRecordsExist_ReturnsNull()
    {
        var queryDate = new DateTime(2023, 01, 02);
        
        var query = new DailyEmailAnalyticsQuery(queryDate);
        Result<DailyEmailAnalytics?> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Null);
        });
    }

    [Test]
    public async Task Handle_WhenMultipleRecordsExist_LogsWarningAndReturnsFirstRecord()
    {
        var testData1 = GlobalTestSetup.Fixture.Create<DailyEmailAnalytics>();
        var testData2 = GlobalTestSetup.Fixture.Create<DailyEmailAnalytics>();
        testData2.Date = testData1.Date;
        await AddTestData([testData1, testData2]);
        
        var query = new DailyEmailAnalyticsQuery(testData1.Date);

        Result<DailyEmailAnalytics?> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(testData1.Id));
        });
    }
}
