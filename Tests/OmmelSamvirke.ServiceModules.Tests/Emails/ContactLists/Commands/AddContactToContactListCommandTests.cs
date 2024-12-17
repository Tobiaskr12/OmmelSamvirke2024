using System.Linq.Expressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class AddContactToContactListCommandTests
{
    private ILogger _logger;
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
        _logger = Substitute.For<ILogger>();
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
        
        _handler = new AddContactToContactListCommandHandler(_contactListRepository, _recipientRepository, _logger);
    }

    [Test]
    public async Task AddContactToContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(Arg.Any<Expression<Func<Recipient, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new List<Recipient>()));

        _contactListRepository
            .UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(_baseValidContactList));

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
            .FindAsync(Arg.Any<Expression<Func<Recipient, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task AddContactToContactListCommand_UpdateFails_ReturnsFailure()
    {
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(Arg.Any<Expression<Func<Recipient, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new List<Recipient>()));

        _contactListRepository
            .UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task AddContactToContactListCommand_ExceptionThrown_ReturnsFailure()
    {
        var command = new AddContactToContactListCommand(_baseValidContactList, _validRecipient);

        _recipientRepository
            .FindAsync(Arg.Any<Expression<Func<Recipient, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
            .Throws(new Exception("Simulated exception"));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }
}
