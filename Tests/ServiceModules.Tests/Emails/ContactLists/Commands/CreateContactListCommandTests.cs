using Contracts.DataAccess.Base;
using Contracts.Emails.ContactLists.Commands;
using FluentResults;
using NSubstitute;
using DomainModules.Emails.Entities;
using ServiceModules.Emails.ContactLists.Commands;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class CreateContactListCommandTests
{
    private IRepository<ContactList> _repository;
    private IRepository<Recipient> _recipientRepository;
    private CreateContactListCommandHandler _handler;

    private readonly ContactList _baseValidContactList = new()
    {
        Name = "Test ContactList",
        Description = "This is a test contact list",
    };

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<IRepository<ContactList>>();
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        
        _handler = new CreateContactListCommandHandler(_repository, _recipientRepository);
    }

    [Test]
    public async Task CreateContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new CreateContactListCommand(_baseValidContactList);
        // Simulate a successful lookup returning no duplicates
        _recipientRepository.FindAsync(default!).ReturnsForAnyArgs(Result.Ok(new List<Recipient>()));
        _repository.AddAsync(_baseValidContactList, Arg.Any<CancellationToken>()).Returns(_baseValidContactList);
        
        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task CreateContactListCommand_DuplicateRecipients_ReplacesContactsAndReturnsSuccess()
    {
        const string duplicateEmail = "duplicate@example.com";
        var duplicateRecipientOne = new Recipient { EmailAddress = duplicateEmail };
        var uniqueRecipient = new Recipient { EmailAddress = "unique@example.com" };
        _baseValidContactList.Contacts = [duplicateRecipientOne, uniqueRecipient];

        // Simulate the repository returning a duplicate recipient
        var duplicateRecipientTwo = new Recipient { EmailAddress = duplicateEmail };
        _recipientRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(Result.Ok(new List<Recipient> { duplicateRecipientTwo }));
        _repository
            .AddAsync(_baseValidContactList, Arg.Any<CancellationToken>())
            .Returns(_baseValidContactList);

        var command = new CreateContactListCommand(_baseValidContactList);
        
        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(_baseValidContactList.Contacts.Any(x => x.EmailAddress == duplicateEmail));
        });
    }
}
