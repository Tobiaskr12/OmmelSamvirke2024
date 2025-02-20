using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.Analytics;
using FluentResults;
using NSubstitute;
using DomainModules.Emails.Entities;
using ServiceModules.Emails.Analytics.Queries;
using ServiceModules.Errors;
using TestHelpers;

namespace ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("UnitTests")]
public class DailyContactListAnalyticsQueryHandlerTests
{
    private IRepository<DailyContactListAnalytics> _repository;
    private DailyContactListAnalyticsQueryHandler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<DailyContactListAnalytics>>();
        _handler = new DailyContactListAnalyticsQueryHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenSingleRecordExists_ReturnsRecordList()
    {
        var queryDate = new DateTime(2023, 01, 01);
        DailyContactListAnalytics expectedAnalytics = CreateTestAnalytics(queryDate, "List One", 100);
        var query = new DailyContactListAnalyticsQuery(queryDate);

        SetupRepositoryFind([expectedAnalytics]);

        Result<List<DailyContactListAnalytics>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.EquivalentTo(new List<DailyContactListAnalytics> { expectedAnalytics }));
        });
    }

    [Test]
    public async Task Handle_WhenNoRecordsExist_ReturnsEmptyList()
    {
        var queryDate = new DateTime(2023, 01, 02);
        var query = new DailyContactListAnalyticsQuery(queryDate);

        SetupRepositoryFind([]);

        Result<List<DailyContactListAnalytics>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryFails_ReturnsGenericError()
    {
        var queryDate = new DateTime(2023, 01, 03);
        var query = new DailyContactListAnalyticsQuery(queryDate);

        _repository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<DailyContactListAnalytics>>());

        Result<List<DailyContactListAnalytics>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task Handle_WhenMultipleRecordsExist_ReturnsAllRecords()
    {
        var queryDate = new DateTime(2023, 01, 04);
        DailyContactListAnalytics analytics1 = CreateTestAnalytics(queryDate, "List One", 50);
        DailyContactListAnalytics analytics2 = CreateTestAnalytics(queryDate, "List Two", 55);
        var query = new DailyContactListAnalyticsQuery(queryDate);

        SetupRepositoryFind([analytics1, analytics2]);

        Result<List<DailyContactListAnalytics>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EquivalentTo(new List<DailyContactListAnalytics> {analytics1, analytics2}));
        });
    }

    private static DailyContactListAnalytics CreateTestAnalytics(DateTime date, string contactListName, int totalContacts) =>
        new()
        {
            Date = date,
            ContactListName = contactListName,
            TotalContacts = totalContacts,
            IsNewsletter = false
        };

    private void SetupRepositoryFind(List<DailyContactListAnalytics> returnList) =>
        _repository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(returnList));
}
