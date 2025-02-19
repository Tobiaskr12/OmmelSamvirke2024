using System.Linq.Expressions;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Entities;
using FluentResults;
using NSubstitute;
using TestDatabaseFixtures;
using TimerTriggers.Emails;

namespace TimerTriggers.Tests.Emails;

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
        var tracer = Substitute.For<ITraceHandler>();
        _function = new DailyContactListAnalyticsFunction(_logger, tracer, _contactListRepository, _dailyAnalyticsRepository);
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

    private void SetupContactListRepository(List<ContactList> contactLists) =>
        _contactListRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(contactLists));
}

[TestFixture, Category("IntegrationTests")]
public class DailyContactListAnalyticsFunctionIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;
    private IRepository<ContactList> _contactListRepository;
    private IRepository<DailyContactListAnalytics> _dailyContactListAnalyticsRepository;
    private ILoggingHandler _loggingHandler;
    private ITraceHandler _traceHandler;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
        _contactListRepository = _integrationTestingHelper.GetService<IRepository<ContactList>>();
        _dailyContactListAnalyticsRepository = _integrationTestingHelper.GetService<IRepository<DailyContactListAnalytics>>();
        _loggingHandler = _integrationTestingHelper.GetService<ILoggingHandler>();
        _traceHandler = _integrationTestingHelper.GetService<ITraceHandler>();
    }
    
    [SetUp]
    public async Task Setup()
    {
        await _integrationTestingHelper.ResetDatabase();
        DateTime yesterday = DateTime.UtcNow.AddDays(-1).Date;

        // ContactList 1 from yesterday with two contacts.
        var contactList1 = new ContactList
        {
            Name = "List One",
            DateCreated = yesterday.AddHours(2),
            Description = "Test list one",
            Contacts =
            [
                new Recipient { EmailAddress = "contact1@example.com" },
                new Recipient { EmailAddress = "contact2@example.com" }
            ]
        };

        // ContactList 2 from yesterday with three contacts.
        var contactList2 = new ContactList
        {
            Name = "List Two",
            DateCreated = yesterday.AddHours(3),
            Description = "Test list two",
            Contacts =
            [
                new Recipient { EmailAddress = "contact3@example.com" },
                new Recipient { EmailAddress = "contact4@example.com" },
                new Recipient { EmailAddress = "contact5@example.com" }
            ]
        };

        // ContactList outside the target range (should not be processed).
        var contactListOutside = new ContactList
        {
            Name = "Old List",
            DateCreated = DateTime.UtcNow.AddDays(-2),
            Description = "Old list",
            Contacts = [new Recipient { EmailAddress = "contact6@example.com" }]
        };

        await _contactListRepository.AddAsync(contactList1);
        await _contactListRepository.AddAsync(contactList2);
        await _contactListRepository.AddAsync(contactListOutside);
    }
    
    [Test]
    public async Task DailyContactListAnalyticsFunction_Runs_CreatesAnalyticsRecords()
    {
        // Arrange
        var function = new DailyContactListAnalyticsFunction(_loggingHandler, _traceHandler, _contactListRepository, _dailyContactListAnalyticsRepository);

        // Act
        await function.Run(null!);

        // Assert
        Result<List<DailyContactListAnalytics>> result = await _dailyContactListAnalyticsRepository.FindAsync(a => a.Date == DateTime.UtcNow.AddDays(-1).Date);
        List<DailyContactListAnalytics>? analyticsList = result.Value;
        DailyContactListAnalytics? listOneAnalytics = analyticsList.FirstOrDefault(a => a.ContactListName == "List One");
        DailyContactListAnalytics? listTwoAnalytics = analyticsList.FirstOrDefault(a => a.ContactListName == "List Two");
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(analyticsList, Has.Count.EqualTo(2));
            Assert.That(listOneAnalytics, Is.Not.Null);
            Assert.That(listTwoAnalytics, Is.Not.Null);
            Assert.That(listOneAnalytics!.TotalContacts, Is.EqualTo(2));
            Assert.That(listTwoAnalytics!.TotalContacts, Is.EqualTo(3));
        });
    }
}
