using System.Linq.Expressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class CreateContactListCommandTests
{
    private ILogger _logger;
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
        _logger = Substitute.For<ILogger>();
        _repository = Substitute.For<IRepository<ContactList>>();
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        
        _handler = new CreateContactListCommandHandler(_repository, _recipientRepository, _logger);
    }

    [Test]
    public async Task CreateContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new CreateContactListCommand(_baseValidContactList);
        // Simulate a successful lookup returning no duplicates
        _recipientRepository.FindAsync(
            Arg.Any<Expression<Func<Recipient, bool>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(new List<Recipient>()));
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
        _recipientRepository.FindAsync(
            Arg.Any<Expression<Func<Recipient, bool>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(new List<Recipient> { duplicateRecipientTwo }));
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

    [Test]
    public async Task CreateContactListCommand_RecipientLookupFails_ReturnsFailure()
    {
        var recipient = new Recipient { EmailAddress = "fail@example.com" };
        _baseValidContactList.Contacts = [recipient];
        
        _recipientRepository.FindAsync(
            Arg.Any<Expression<Func<Recipient, bool>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Fail<List<Recipient>>(ErrorMessages.GenericErrorWithRetryPrompt));

        var command = new CreateContactListCommand(_baseValidContactList);
        
        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
}
