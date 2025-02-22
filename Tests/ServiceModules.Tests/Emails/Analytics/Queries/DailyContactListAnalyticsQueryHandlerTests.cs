using AutoFixture;
using Contracts.ServiceModules.Emails.Analytics;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("IntegrationTests")]
public class DailyContactListAnalyticsQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_WhenSingleRecordExists_ReturnsRecordList()
    {
        var testData = GlobalTestSetup.Fixture.Create<DailyContactListAnalytics>();
        await AddTestData(testData);
        
        var query = new DailyContactListAnalyticsQuery(testData.Date);

        Result<List<DailyContactListAnalytics>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value[0].Id, Is.EqualTo(testData.Id));
        });
    }

    [Test]
    public async Task Handle_WhenNoRecordsExist_ReturnsEmptyList()
    {
        var queryDate = new DateTime(2023, 01, 02);
        var query = new DailyContactListAnalyticsQuery(queryDate);
    
        Result<List<DailyContactListAnalytics>> result = await GlobalTestSetup.Mediator.Send(query);
    
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
    
    [Test]
    public async Task Handle_WhenMultipleRecordsExist_ReturnsAllRecords()
    {
        var testData1 = GlobalTestSetup.Fixture.Create<DailyContactListAnalytics>();
        var testData2 = GlobalTestSetup.Fixture.Create<DailyContactListAnalytics>();
        testData2.Date = testData1.Date;
        await AddTestData([testData1, testData2]);
        
        var query = new DailyContactListAnalyticsQuery(testData1.Date);
    
        Result<List<DailyContactListAnalytics>> result = await GlobalTestSetup.Mediator.Send(query);
    
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(result.Value[0].Id, Is.EqualTo(testData1.Id));
            Assert.That(result.Value[1].Id, Is.EqualTo(testData2.Id));
        });
    }
}
