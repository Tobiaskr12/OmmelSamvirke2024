using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("UnitTests")]
public class CountContactsInContactListQueryHandlerTests
{
    private IRepository<ContactList> _repository;
    private ILogger _logger;
    private CountContactsInContactListQueryHandler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly ContactList _testContactList = new()
    {
        Id = 1,
        Name = "Test ContactList",
        Description = "A test contact list",
        Contacts =
        [
            new Recipient { EmailAddress = "test1@example.com" },
            new Recipient { EmailAddress = "test2@example.com" },
            new Recipient { EmailAddress = "test3@example.com" }
        ]
    };

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<ContactList>>();
        _logger = Substitute.For<ILogger>();
        _handler = new CountContactsInContactListQueryHandler(_repository, _logger);
    }

    [Test]
    public async Task Handle_WhenContactListExists_ReturnsContactCount()
    {
        var query = new CountContactsInContactListQuery(1);

        _repository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Ok(_testContactList)));
        
        Result<int> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(_testContactList.Contacts.Count));
        });
    }

    [Test]
    public async Task Handle_WhenContactListDoesNotExist_ReturnsFailure()
    {
        var query = new CountContactsInContactListQuery(99);

        _repository
            .GetByIdAsync(99, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Fail<ContactList>("Not found")));
        
        Result<int> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryFails_ReturnsGenericError()
    {
        var query = new CountContactsInContactListQuery(1);
        const string failureMessage = "Database error";

        _repository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Fail<ContactList>(new List<string> { failureMessage })));
        
        Result<int> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task Handle_WhenExceptionThrown_ReturnsFailureWithErrorCode()
    {
        var query = new CountContactsInContactListQuery(1);

        _repository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns<Task<Result<ContactList>>>(_ => throw new Exception("Simulated exception"));
        
        Result<int> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Does.StartWith(ErrorMessages.GenericErrorWithErrorCode));
            _logger.Received(1).Log(
                Arg.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Length > 0),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()!
            );
        });
    }
}

[TestFixture, Category("IntegrationTests")]
public class CountContactsInContactListIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
    }

    private async Task<int> SeedTestData()
    {
        await _integrationTestingHelper.ResetDatabase();
        
        var contactListRepository = _integrationTestingHelper.ServiceProvider.GetService(typeof(IRepository<ContactList>)) as IRepository<ContactList>;
        var recipientRepository = _integrationTestingHelper.ServiceProvider.GetService(typeof(IRepository<Recipient>)) as IRepository<Recipient>;
        if (contactListRepository == null) throw new ArgumentNullException(nameof(contactListRepository));
        if (recipientRepository == null) throw new ArgumentNullException(nameof(contactListRepository));

        List<Recipient> contacts =
        [
            new() { EmailAddress = "integration1@example.com" },
            new() { EmailAddress = "integration2@example.com" },
            new() { EmailAddress = "integration3@example.com" }
        ];
        var contactList = new ContactList
        {
            Name = "Integration Test ContactList",
            Description = "A contact list for integration testing",
            Contacts = contacts
        };

        await recipientRepository.AddAsync(contacts);
        Result<ContactList> createdContactList = await contactListRepository.AddAsync(contactList);
        if (createdContactList.IsFailed) throw new Exception("Failed to create ContactList for integration test");

        return createdContactList.Value.Id;
    }

    [Test]
    public async Task CountContactsInContactList_ValidId_ReturnsContactCount()
    {
        int contactListId = await SeedTestData();
        var query = new CountContactsInContactListQuery(contactListId);
        
        Result<int> result = await _integrationTestingHelper.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(3));
        });
    }
}
