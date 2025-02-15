using Contracts.DataAccess.Base;
using Contracts.Emails.ContactLists.Queries;
using FluentResults;
using NSubstitute;
using DomainModules.Emails.Entities;
using ServiceModules.Emails.ContactLists.Queries;
using TestDatabaseFixtures;

namespace ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("UnitTests")]
public class GetContactListQueryTests
{
    private IRepository<ContactList> _contactListRepository;
    private GetContactListQueryHandler _handler;

    private readonly ContactList _testContactList = new()
    {
        Id = 1,
        Name = "Test ContactList",
        Description = "A test contact list",
        Contacts = []
    };

    [SetUp]
    public void Setup()
    {
        _contactListRepository = Substitute.For<IRepository<ContactList>>();

        _handler = new GetContactListQueryHandler(_contactListRepository);
    }

    [Test]
    public async Task GetContactList_ValidId_ReturnsSuccess()
    {
        var query = new GetContactListQuery(1);

        _contactListRepository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Ok(_testContactList));

        Result<ContactList> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(_testContactList));
        });
    }

    [Test]
    public async Task GetContactList_IdNotFound_ReturnsFailure()
    {
        var query = new GetContactListQuery(99);

        _contactListRepository
            .GetByIdAsync(99, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Not found"));

        Result<ContactList> result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
}

[TestFixture, Category("IntegrationTests")]
public class GetContactListIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
    }

    private async Task<int> SeedTestData()
    {
        var contactListRepository = _integrationTestingHelper.GetService<IRepository<ContactList>>();
        await _integrationTestingHelper.ResetDatabase();
        
        var contactList = new ContactList
        {
            Name = "Test ContactList",
            Description = "A test contact list",
            Contacts =
            [
                new Recipient { EmailAddress = "test1@example.com" },
                new Recipient { EmailAddress = "test2@example.com" }
            ]
        };

        Result<ContactList> createdEntity = await contactListRepository.AddAsync(contactList);
        if (createdEntity.IsFailed) throw new Exception("Failed to create contact list for seed data");

        return createdEntity.Value.Id;
    }

    [Test]
    public async Task GetContactList_ValidId_ReturnsSuccess()
    {
        int contactListId = await SeedTestData();
        var query = new GetContactListQuery(contactListId);

        Result<ContactList> result = await _integrationTestingHelper.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Id, Is.EqualTo(contactListId));
            Assert.That(result.Value.Name, Is.EqualTo("Test ContactList"));
            Assert.That(result.Value.Contacts, Has.Count.EqualTo(2));
            Assert.That(result.Value.Contacts[0].EmailAddress, Is.EqualTo("test1@example.com"));
            Assert.That(result.Value.Contacts[1].EmailAddress, Is.EqualTo("test2@example.com"));
        });
    }

    [Test]
    public async Task GetContactList_IdNotFound_ReturnsFailure()
    {
        int contactListId = await SeedTestData();
        var query = new GetContactListQuery(contactListId + 1);

        Result<ContactList> result = await _integrationTestingHelper.Mediator.Send(query);

        Assert.That(result.IsFailed);
    }
}
