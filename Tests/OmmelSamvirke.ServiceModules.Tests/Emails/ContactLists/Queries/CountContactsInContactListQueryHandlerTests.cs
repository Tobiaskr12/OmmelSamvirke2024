using Contracts.DataAccess.Base;
using FluentResults;
using NSubstitute;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;
using OmmelSamvirke.ServiceModules.Errors;
using TestDatabaseFixtures;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("UnitTests")]
public class CountContactsInContactListQueryHandlerTests
{
    private IRepository<ContactList> _repository;
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
        _handler = new CountContactsInContactListQueryHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenContactListExists_ReturnsContactCount()
    {
        var query = new CountContactsInContactListQuery(1);

        _repository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(MockHelpers.SuccessAsyncResult(_testContactList));
        
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
            .Returns(MockHelpers.FailedAsyncResult<ContactList>());
        
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

        _repository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(MockHelpers.FailedAsyncResult<ContactList>());
        
        Result<int> result = await _handler.Handle(query, _cancellationToken);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
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
        
        var contactListRepository = _integrationTestingHelper.GetService<IRepository<ContactList>>();
        var recipientRepository = _integrationTestingHelper.GetService<IRepository<Recipient>>();

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
