using AutoFixture;
using Contracts.DataAccess.Emails.Enums;
using Contracts.ServiceModules.Emails.Analytics;
using FluentResults;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("IntegrationTests")]
public class EmailsSentInIntervalQueryTests : ServiceTestBase
{
    [Test]
    public async Task EmailsSentInIntervalQuery_ValidPerMinute_ReturnsEmailCount()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow.AddMinutes(-10);
        List<Email> testEmailsInInterval =
        [
            GlobalTestSetup.Fixture.Create<Email>(),
            GlobalTestSetup.Fixture.Create<Email>()
        ];
        testEmailsInInterval[0].DateCreated = startTime.AddSeconds(10);
        testEmailsInInterval[1].DateCreated = startTime.AddSeconds(20);
        await AddTestData(testEmailsInInterval);
        
        List<Email> testEmailsOutsideInterval =
        [
            GlobalTestSetup.Fixture.Create<Email>(),
            GlobalTestSetup.Fixture.Create<Email>(),
            GlobalTestSetup.Fixture.Create<Email>()
        ];
        testEmailsOutsideInterval[0].DateCreated = startTime.AddSeconds(70);
        testEmailsOutsideInterval[1].DateCreated = startTime.AddMinutes(2);
        testEmailsOutsideInterval[2].DateCreated = startTime.AddMinutes(20);
        await AddTestData(testEmailsOutsideInterval);
        
        // Act 
        var query = new EmailsSentInIntervalQuery(startTime, ServiceLimitInterval.PerMinute);
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(testEmailsInInterval.Count));
        });
    }

    [Test]
    public async Task EmailsSentInIntervalQuery_ValidPerHour_ReturnsEmailCount()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow.AddHours(-2);
        List<Email> testEmailsInInterval =
        [
            GlobalTestSetup.Fixture.Create<Email>(),
            GlobalTestSetup.Fixture.Create<Email>()
        ];
        testEmailsInInterval[0].DateCreated = startTime.AddMinutes(10);
        testEmailsInInterval[1].DateCreated = startTime.AddMinutes(20);
        testEmailsInInterval[1].DateCreated = startTime.AddMinutes(30);
        await AddTestData(testEmailsInInterval);

        var testEmailOutsideInterval = GlobalTestSetup.Fixture.Create<Email>();
        testEmailOutsideInterval.DateCreated = startTime.AddHours(2);
        await AddTestData(testEmailOutsideInterval);
        
        // Act
        var query = new EmailsSentInIntervalQuery(startTime, ServiceLimitInterval.PerHour);
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);
    
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(testEmailsInInterval.Count));
        });
    }
    
    [Test]
    public async Task EmailsSentInIntervalQuery_InvalidInterval_ReturnsFailure()
    {
        DateTime startTime = DateTime.UtcNow.AddMinutes(-10);
        const ServiceLimitInterval invalidInterval = (ServiceLimitInterval)99;
        
        var query = new EmailsSentInIntervalQuery(startTime, invalidInterval);
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }
    
    [Test]
    public async Task EmailsSentInIntervalQuery_StartTimeInFuture_ReturnsFailure()
    {
        DateTime futureStartTime = DateTime.UtcNow.AddMinutes(5);
        var query = new EmailsSentInIntervalQuery(futureStartTime, ServiceLimitInterval.PerMinute);
        
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }
}
