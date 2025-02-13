using Contracts.DataAccess.Emails.Enums;
using FluentResults;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.ServiceModules.Emails.Sending.Queries;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Sending.Queries;

[TestFixture, Category("UnitTests")]
public class ServiceLimitsQueryTests
{
    private ServiceLimitsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new ServiceLimitsQueryHandler();
    }

    [Test]
    public async Task ServiceLimitsQuery_PerMinute_ReturnsExpectedLimit()
    {
        var query = new ServiceLimitsQuery(ServiceLimitInterval.PerMinute);

        Result<int> result = await _handler.Handle(query, CancellationToken.None);

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

        Result<int> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(ServiceLimits.EmailsPerHour));
        });
    }

    [Test]
    public async Task ServiceLimitsQuery_InvalidInterval_ReturnsFailure()
    {
        var query = new ServiceLimitsQuery((ServiceLimitInterval)99);

        Result<int> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }
}
