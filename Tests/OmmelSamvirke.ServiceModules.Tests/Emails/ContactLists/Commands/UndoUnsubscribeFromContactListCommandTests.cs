using System.Linq.Expressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class UndoUnsubscribeFromContactListCommandTests
{
    private ILogger _logger;
    private IRepository<ContactList> _contactListRepository;
    private IRepository<ContactListUnsubscription> _unsubscriptionRepository;
    private UndoUnsubscribeFromContactListCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger>();
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _unsubscriptionRepository = Substitute.For<IRepository<ContactListUnsubscription>>();
        _handler = new UndoUnsubscribeFromContactListCommandHandler(_logger, _contactListRepository, _unsubscriptionRepository);
    }
    
    private void ConfigureUnsubscriptionFindAsync(Result<List<ContactListUnsubscription>> result) =>
         _unsubscriptionRepository.FindAsync(
             Arg.Any<Expression<Func<ContactListUnsubscription, bool>>>(),
             Arg.Any<bool>(),
             Arg.Any<CancellationToken>()
         ).Returns(Task.FromResult(result));
    
    private void ConfigureContactListFindAsync(Result<List<ContactList>> result) =>
         _contactListRepository.FindAsync(
             Arg.Any<Expression<Func<ContactList, bool>>>(),
             Arg.Any<bool>(),
             Arg.Any<CancellationToken>()
         ).Returns(Task.FromResult(result));
    
    private void ConfigureContactListUpdateAsync(ContactList expectedContactList, Result<ContactList> result) =>
         _contactListRepository.UpdateAsync(
             expectedContactList,
             Arg.Any<CancellationToken>()
         ).Returns(Task.FromResult(result));
    
    [Test]
    public async Task UndoUnsubscribe_RecordNotFound_ReturnsFailure()
    {
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), Guid.NewGuid());
        ConfigureUnsubscriptionFindAsync(Result.Ok(new List<ContactListUnsubscription>()));
        Result result = await _handler.Handle(command, CancellationToken.None);
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_RecordExpired_ReturnsFailure()
    {
        // Create a record with required properties.
        var unsubscription = new ContactListUnsubscription 
        { 
            DateCreated = DateTime.UtcNow.AddDays(-15),
            EmailAddress = "test@example.com",
            ContactListId = 1
        };
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), unsubscription.UndoToken);
        // Also, simulate finding a contact list with ID = 1.
        var contactList = new ContactList
        {
            Id = 1,
            Name = "List1",
            Description = "First list",
            Contacts = []
        };
        ConfigureUnsubscriptionFindAsync(Result.Ok(new List<ContactListUnsubscription> { unsubscription }));
        ConfigureContactListFindAsync(Result.Ok(new List<ContactList> { contactList }));
        Result result = await _handler.Handle(command, CancellationToken.None);
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_ContactListNotFound_ReturnsFailure()
    {
        var unsubscription = new ContactListUnsubscription 
        { 
            DateCreated = DateTime.UtcNow,
            EmailAddress = "test@example.com",
            ContactListId = 1
        };
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), unsubscription.UndoToken);
        ConfigureUnsubscriptionFindAsync(Result.Ok(new List<ContactListUnsubscription> { unsubscription }));
        // Simulate contact list not found.
        ConfigureContactListFindAsync(Result.Ok(new List<ContactList>()));
        Result result = await _handler.Handle(command, CancellationToken.None);
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_UpdateFails_ReturnsFailure()
    {
        var unsubscription = new ContactListUnsubscription 
        {
            DateCreated = DateTime.UtcNow,
            EmailAddress = "test@example.com",
            ContactListId = 1
        };
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), unsubscription.UndoToken);
        var contactList = new ContactList
        {
            Id = 1,
            Name = "List1",
            Description = "First list",
            Contacts = []
        };
        ConfigureUnsubscriptionFindAsync(Result.Ok(new List<ContactListUnsubscription> { unsubscription }));
        ConfigureContactListFindAsync(Result.Ok(new List<ContactList> { contactList }));
        ConfigureContactListUpdateAsync(contactList, Result.Fail<ContactList>("Update failed"));
        Result result = await _handler.Handle(command, CancellationToken.None);
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_Success_AddsRecipientAndReturnsSuccess()
    {
        var unsubscription = new ContactListUnsubscription 
        { 
            DateCreated = DateTime.UtcNow,
            EmailAddress = "test@example.com",
            ContactListId = 1
        };
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), unsubscription.UndoToken);
        var contactList = new ContactList
        {
            Id = 1,
            Name = "List1",
            Description = "First list",
            Contacts = []
        };
        ConfigureUnsubscriptionFindAsync(Result.Ok(new List<ContactListUnsubscription> { unsubscription }));
        ConfigureContactListFindAsync(Result.Ok(new List<ContactList> { contactList }));
        
        // Simulate update: contact list now has the recipient added.
        var updatedContactList = new ContactList
        {
            Id = 1,
            Name = contactList.Name,
            Description = contactList.Description,
            Contacts = [new Recipient { EmailAddress = command.EmailAddress }]
        };
        ConfigureContactListUpdateAsync(contactList, Result.Ok(updatedContactList));
        Result result = await _handler.Handle(command, CancellationToken.None);
        Assert.That(result.IsSuccess);
    }
    
    [Test]
    public async Task UndoUnsubscribe_ExceptionThrown_ReturnsFailure()
    {
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), Guid.NewGuid());
        _unsubscriptionRepository.FindAsync(
            Arg.Any<Expression<Func<ContactListUnsubscription, bool>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns<Task<Result<List<ContactListUnsubscription>>>>(_ => throw new Exception("Simulated exception"));
    
        Result result = await _handler.Handle(command, CancellationToken.None);
    
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
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
public class UndoUnsubscribeFromContactListIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;

    [OneTimeSetUp]
    public void OneTimeSetUp() =>
        _integrationTestingHelper = new IntegrationTestingHelper();

    [SetUp]
    public async Task Setup() =>
        await _integrationTestingHelper.ResetDatabase();

    [Test]
    public async Task UndoUnsubscribeFromContactList_HappyPath_ReturnsSuccess_AndDeletesUnsubscriptionRecord()
    {
        // Seed a contact list
        var contactListRepository = _integrationTestingHelper.ServiceProvider.GetService(typeof(IRepository<ContactList>)) as IRepository<ContactList>;
        if (contactListRepository == null) throw new Exception("ContactList repository not found");
        var contactList = new ContactList
        {
            Name = "Integration List",
            Description = "Integration test contact list",
            Contacts = [new Recipient { EmailAddress = "ommelsamvirketest2@gmail.com" }]
        };
        Result<ContactList> addContactListResult = await contactListRepository.AddAsync(contactList);
        Assert.That(addContactListResult.IsSuccess, Is.True);

        // Seed a valid unsubscription record with matching email and contact list id.
        const string email = "ommelsamvirketest1@gmail.com";
        var unsubscriptionRepository = _integrationTestingHelper.ServiceProvider.GetService(typeof(IRepository<ContactListUnsubscription>)) as IRepository<ContactListUnsubscription>;
        if (unsubscriptionRepository == null) throw new Exception("ContactListUnsubscription repository not found");
        var unsubscription = new ContactListUnsubscription
        {
            EmailAddress = email,
            ContactListId = addContactListResult.Value.Id,
            DateCreated = DateTime.UtcNow
        };
        Result<ContactListUnsubscription> unsubscriptionAddResult = await unsubscriptionRepository.AddAsync(unsubscription);
        Assert.That(unsubscriptionAddResult.IsSuccess, Is.True);

        // Undo unsubscription
        var command = new UndoUnsubscribeFromContactListCommand(email, addContactListResult.Value.UnsubscribeToken, unsubscriptionAddResult.Value.UndoToken);
        Result result = await _integrationTestingHelper.Mediator.Send(command);
        Assert.That(result.IsSuccess, Is.True);

        // Check that the contact list contains the recipient
        Result<ContactList> fetchedContactList = await contactListRepository.GetByIdAsync(addContactListResult.Value.Id);
        Assert.Multiple(() =>
        {
            Assert.That(fetchedContactList.IsSuccess, Is.True);
            Assert.That(fetchedContactList.Value.Contacts.Exists(r => r.EmailAddress == email), Is.True);
        });

        // Check that the undo record has been deleted
        Result<List<ContactListUnsubscription>> allUnsubscriptions = await unsubscriptionRepository.GetAllAsync();
        Assert.Multiple(() =>
        {
            Assert.That(allUnsubscriptions.IsSuccess, Is.True);
            Assert.That(allUnsubscriptions.Value.Exists(u => u.UndoToken == unsubscriptionAddResult.Value.UndoToken), Is.False);
        });
    }
}
