using System.Linq.Expressions;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using NSubstitute;
using OmmelSamvirke.DomainModules.Emails.Entities;
using TestDatabaseFixtures;

namespace OmmelSamvirke.TimerTriggers.Tests;

[TestFixture, Category("UnitTests")]
public class DailyContactListAnalyticsFunctionTests
{
    private IRepository<ContactList> _contactListRepository;
    private IRepository<DailyContactListAnalytics> _dailyAnalyticsRepository;
    private ILoggingHandler _logger;
    private DailyContactListAnalyticsFunction _function;

    [SetUp]
    public void Setup()
    {
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _dailyAnalyticsRepository = Substitute.For<IRepository<DailyContactListAnalytics>>();
        _logger = Substitute.For<ILoggingHandler>();
        _function = new DailyContactListAnalyticsFunction(_logger, _contactListRepository, _dailyAnalyticsRepository);
    }

    [Test]
    public void Run_WhenContactListRetrievalFails_ThrowsException()
    {
        _contactListRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<ContactList>>());
        _dailyAnalyticsRepository
            .AddAsync(Arg.Any<DailyContactListAnalytics>())
            .Returns(MockHelpers.SuccessAsyncResult(CreateTestDailyAnalytics("Dummy", DateTime.UtcNow, 0)));
        
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<Exception>(async () => await _function.Run(null!));
            _contactListRepository.Received(1).FindAsync(Arg.Any<Expression<Func<ContactList, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
            _dailyAnalyticsRepository.DidNotReceive().AddAsync(Arg.Any<DailyContactListAnalytics>());
        });
    }

    [Test]
    public void Run_WhenSavingAnalyticsFails_ThrowsException()
    {
        ContactList contactList = CreateTestContactList("TestList", "Test description", 5);
        SetupContactListRepository([contactList]);
        _dailyAnalyticsRepository
            .AddAsync(Arg.Any<List<DailyContactListAnalytics>>())
            .Returns(MockHelpers.FailedAsyncResult<List<DailyContactListAnalytics>>());
        
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<Exception>(async () => await _function.Run(null!));
            _contactListRepository.Received(1).FindAsync(Arg.Any<Expression<Func<ContactList, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
            _dailyAnalyticsRepository.Received(1).AddAsync(Arg.Any<List<DailyContactListAnalytics>>());
        });
    }

    [Test]
    public void Run_WhenEverythingSucceeds_CompletesSuccessfully()
    {
        ContactList contactList1 = CreateTestContactList("List One", "Description One", 10);
        ContactList contactList2 = CreateTestContactList("List Two", "Description Two", 15);
        SetupContactListRepository([contactList1, contactList2]);

        DailyContactListAnalytics analytics1 = CreateTestContactListAnalytics(contactList1);
        DailyContactListAnalytics analytics12= CreateTestContactListAnalytics(contactList1);
        List<DailyContactListAnalytics> expectedAnalytics = [analytics1, analytics12];
        
        _dailyAnalyticsRepository
            .AddAsync(Arg.Any<List<DailyContactListAnalytics>>())
            .Returns(MockHelpers.SuccessAsyncResult(expectedAnalytics));

        Assert.Multiple(() =>
        {
            Assert.DoesNotThrowAsync(async () => await _function.Run(null!));
            _contactListRepository.Received(1)
                .FindAsync(
                    Arg.Any<Expression<Func<ContactList, bool>>>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>()
                );
            _dailyAnalyticsRepository.Received(1).AddAsync(Arg.Any<List<DailyContactListAnalytics>>());
        });
    }

    private static ContactList CreateTestContactList(string name, string description, int numberOfContacts) =>
        new()
        {
            Name = name,
            Description = description,
            Contacts = Enumerable.Range(0, numberOfContacts).Select(_ => CreateTestRecipient()).ToList()
        };

    private static Recipient CreateTestRecipient() =>
        new()
        {
            EmailAddress = Guid.NewGuid() + "@example.com"
        };

    private static DailyContactListAnalytics CreateTestContactListAnalytics(ContactList contactList) =>
        new()
        {
            ContactListName = contactList.Name,
            TotalContacts = contactList.Contacts.Count,
            Date = DateTime.UtcNow.AddDays(-1).Date,
            IsNewsletter = false
        };

    private static DailyContactListAnalytics CreateTestDailyAnalytics(string name, DateTime date, int totalContacts) =>
        new()
        {
            ContactListName = name,
            Date = date.Date,
            TotalContacts = totalContacts,
            IsNewsletter = false
        };

    private void SetupContactListRepository(List<ContactList> contactLists) =>
        _contactListRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(contactLists));
}
