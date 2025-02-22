using Contracts.DataAccess.Emails.Enums;
using Contracts.ServiceModules.Emails.Sending;
using FluentResults;
using DomainModules.Emails.Constants;

namespace ServiceModules.Tests.Emails.Sending.Queries;

[TestFixture, Category("IntegrationTests")]
public class ServiceLimitsQueryTests : ServiceTestBase
{
    [Test]
    public async Task ServiceLimitsQuery_PerMinute_ReturnsExpectedLimit()
    {
        var query = new ServiceLimitsQuery(ServiceLimitInterval.PerMinute);
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(ServiceLimits.EmailsPerMinute));
        });
    }

    [Test]
    public async Task ServiceLimitsQuery_PerHour_ReturnsExpectedLimit()
    {
        var query = new ServiceLimitsQuery(ServiceLimitInterval.PerHour);
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(ServiceLimits.EmailsPerHour));
        });
    }
}
