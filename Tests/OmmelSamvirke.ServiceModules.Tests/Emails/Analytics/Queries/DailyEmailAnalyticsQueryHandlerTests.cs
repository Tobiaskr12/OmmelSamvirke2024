using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.Analytics.Queries;
using OmmelSamvirke.ServiceModules.Errors;
using TestDatabaseFixtures;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("UnitTests")]
public class DailyEmailAnalyticsQueryHandlerTests
{
    private IRepository<DailyEmailAnalytics> _repository;
    private ILogger _logger;
    private DailyEmailAnalyticsQueryHandler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<DailyEmailAnalytics>>();
        _logger = Substitute.For<ILogger>();
        _handler = new DailyEmailAnalyticsQueryHandler(_repository, _logger);
    }

    [Test]
    public async Task Handle_WhenSingleRecordExists_ReturnsRecord()
    {
        var queryDate = new DateTime(2023, 01, 01);
        DailyEmailAnalytics expectedAnalytics = CreateTestAnalytics(queryDate, 100, 500);
        var query = new DailyEmailAnalyticsQuery(queryDate);

        SetupRepositoryFind([expectedAnalytics]);

        Result<DailyEmailAnalytics?> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(expectedAnalytics));
        });
    }

    [Test]
    public async Task Handle_WhenNoRecordsExist_ReturnsNull()
    {
        var queryDate = new DateTime(2023, 01, 02);
        var query = new DailyEmailAnalyticsQuery(queryDate);

        SetupRepositoryFind([]);

        Result<DailyEmailAnalytics?> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Null);
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryFails_ReturnsGenericError()
    {
        var queryDate = new DateTime(2023, 01, 03);
        var query = new DailyEmailAnalyticsQuery(queryDate);

        _repository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<DailyEmailAnalytics>>());

        Result<DailyEmailAnalytics?> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task Handle_WhenMultipleRecordsExist_LogsWarningAndReturnsFirstRecord()
    {
        var queryDate = new DateTime(2023, 01, 04);
        DailyEmailAnalytics analytics1 = CreateTestAnalytics(queryDate, 50, 200);
        DailyEmailAnalytics analytics2 = CreateTestAnalytics(queryDate, 55, 220);
        var query = new DailyEmailAnalyticsQuery(queryDate);

        SetupRepositoryFind([analytics1, analytics2]);

        Result<DailyEmailAnalytics?> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(analytics1));
            _logger.Received(1).Log(
                Arg.Is<LogLevel>(lvl => lvl == LogLevel.Warning),
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains("analytics entities were found")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()!
            );
        });
    }

    private static DailyEmailAnalytics CreateTestAnalytics(DateTime date, int sentEmails, int totalRecipients) =>
        new()
        {
            Date = date,
            SentEmails = sentEmails,
            TotalRecipients = totalRecipients
        };

    private void SetupRepositoryFind(List<DailyEmailAnalytics> returnList) =>
        _repository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(returnList));
}
