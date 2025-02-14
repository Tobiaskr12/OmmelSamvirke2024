using Contracts.DataAccess.Base;
using FluentResults;
using NSubstitute;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Queries;
using OmmelSamvirke.ServiceModules.Errors;
using TestDatabaseFixtures;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("UnitTests")]
public class SearchContactListsByEmailQueryTests
{
    private IRepository<ContactList> _repository;
    private SearchContactListsByEmailQueryHandler _handler;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<ContactList>>();
        _handler = new SearchContactListsByEmailQueryHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenContactListsExist_ReturnsContactLists()
    {
        const string emailAddress = "test@example.com";
        ContactList contactList1 = CreateTestContactList(1, "List One", "Description One", 
        [
            CreateTestRecipient(emailAddress),
            CreateTestRecipient("other1@example.com")
        ]);
        ContactList contactList2 = CreateTestContactList(2, "List Two", "Description Two", 
        [
            CreateTestRecipient(emailAddress),
            CreateTestRecipient("other2@example.com")
        ]);
        var query = new SearchContactListsByEmailQuery(emailAddress);

        SetupRepositoryFind([contactList1, contactList2]);

        Result<List<ContactList>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.EquivalentTo(new List<ContactList> { contactList1, contactList2 }));
        });
    }

    [Test]
    public async Task Handle_WhenNoContactListsExist_ReturnsEmptyList()
    {
        const string emailAddress = "nonexistent@example.com";
        var query = new SearchContactListsByEmailQuery(emailAddress);

        SetupRepositoryFind([]);

        Result<List<ContactList>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_WhenRepositoryFails_ReturnsGenericError()
    {
        const string emailAddress = "error@example.com";
        var query = new SearchContactListsByEmailQuery(emailAddress);

        _repository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<ContactList>>());

        Result<List<ContactList>> result = await _handler.Handle(query, _cancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    private static ContactList CreateTestContactList(int id, string name, string description, List<Recipient> contacts) =>
        new()
        {
            Id = id,
            Name = name,
            Description = description,
            Contacts = contacts
        };

    private static Recipient CreateTestRecipient(string emailAddress) =>
        new()
        {
            EmailAddress = emailAddress
        };

    private void SetupRepositoryFind(List<ContactList> returnList) =>
        _repository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(returnList));
}

[TestFixture, Category("IntegrationTests")]
public class SearchContactListsByEmailIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
    }

    private async Task SeedTestData(string emailAddress)
    {
        await _integrationTestingHelper.ResetDatabase();
        
        var contactList1 = new ContactList
        {
            Name = "Integration List One",
            Description = "First integration test contact list",
            Contacts =
            [
                new Recipient { EmailAddress = emailAddress },
                new Recipient { EmailAddress = "other1@example.com" }
            ]
        };

        var contactList2 = new ContactList
        {
            Name = "Integration List Two",
            Description = "Second integration test contact list",
            Contacts =
            [
                new Recipient { EmailAddress = emailAddress },
                new Recipient { EmailAddress = "other2@example.com" }
            ]
        };

        var contactList3 = new ContactList
        {
            Name = "Integration List Three",
            Description = "Third integration test contact list",
            Contacts = [new Recipient { EmailAddress = "unique@example.com" }]
        };
        
        await _integrationTestingHelper.Mediator.Send(new CreateContactListCommand(contactList1));
        await _integrationTestingHelper.Mediator.Send(new CreateContactListCommand(contactList2));
        await _integrationTestingHelper.Mediator.Send(new CreateContactListCommand(contactList3));
    }

    [Test]
    public async Task SearchContactListsByEmail_ExistingEmail_ReturnsContactLists()
    {
        const string searchEmail = "search@example.com";
        await SeedTestData(searchEmail);

        var query = new SearchContactListsByEmailQuery(searchEmail);

        Result<List<ContactList>> result = await _integrationTestingHelper.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(result.Value.Exists(cl => cl.Name == "Integration List One"), Is.True);
            Assert.That(result.Value.Exists(cl => cl.Name == "Integration List Two"), Is.True);
        });
    }

    [Test]
    public async Task SearchContactListsByEmail_NonExistingEmail_ReturnsEmptyList()
    {
        const string searchEmail = "nonexistent@example.com";
        await SeedTestData("search@example.com");

        var query = new SearchContactListsByEmailQuery(searchEmail);

        Result<List<ContactList>> result = await _integrationTestingHelper.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
