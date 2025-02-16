using Contracts.DataAccess.Base;
using Contracts.Emails.ContactLists.Commands;
using FluentResults;
using NSubstitute;
using DomainModules.Emails.Entities;
using ServiceModules.Emails.ContactLists.Commands;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class AddContactToContactListCommandTests
{
    private IRepository<ContactList> _contactListRepository;
    private IRepository<Recipient> _recipientRepository;
    private AddContactToContactListCommandHandler _handler;

    private readonly ContactList _baseValidContactList = new()
    {
        Name = "Test ContactList",
        Description = "A test contact list",
        Contacts = []
    };

    private readonly Recipient _validRecipient = new()
    {
        EmailAddress = "valid@example.com"
    };

    [SetUp]
    public void Setup()
    {
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        
        _handler = new AddContactToContactListCommandHandler(_contactListRepository, _recipientRepository);
    }

    [Test]
    public async Task AddContactToContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(Result.Ok(new List<Recipient>()));

        _contactListRepository
            .UpdateAsync(
                Arg.Any<ContactList>(),
                Arg.Any<CancellationToken>()
            ).Returns(Result.Ok(_baseValidContactList));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(_baseValidContactList.Contacts, Contains.Item(_validRecipient));
        });
    }

    [Test]
    public async Task AddContactToContactListCommand_RecipientQueryFails_ReturnsFailure()
    {
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(Result.Fail<List<Recipient>>(ErrorMessages.GenericErrorWithRetryPrompt));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task AddContactToContactListCommand_UpdateFails_ReturnsFailure()
    {
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(Result.Ok(new List<Recipient>()));

        _contactListRepository
            .UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<ContactList>(ErrorMessages.GenericErrorWithRetryPrompt));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task AddContactToContactListCommand_DuplicateRecipient_ReplacesContact()
    {
        _baseValidContactList.Contacts.Add(_validRecipient);
        var duplicateRecipient = new Recipient { EmailAddress = _validRecipient.EmailAddress };
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(Result.Ok(new List<Recipient> { duplicateRecipient }));

        _contactListRepository
            .UpdateAsync(
                Arg.Any<ContactList>(),
                Arg.Any<CancellationToken>()
            ).Returns(Result.Ok(_baseValidContactList));
        
        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(_baseValidContactList.Contacts.First(), Is.EqualTo(_validRecipient));
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.ContactList_AddContact_ContactAlreadyExitsts));
        });
    }
}
