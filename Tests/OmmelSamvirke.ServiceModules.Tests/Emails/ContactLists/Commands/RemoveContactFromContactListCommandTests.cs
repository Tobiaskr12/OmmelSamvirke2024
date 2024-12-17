using System.Linq.Expressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;
using OmmelSamvirke.ServiceModules.Errors;
using MediatR;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class RemoveContactFromContactListCommandTests
{
    private ILogger _logger;
    private IRepository<ContactList> _contactListRepository;
    private IRepository<Recipient> _contactRepository;
    private IMediator _mediator;
    private RemoveContactFromContactListCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger>();
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _contactRepository = Substitute.For<IRepository<Recipient>>();
        _mediator = Substitute.For<IMediator>();

        _handler = new RemoveContactFromContactListCommandHandler(
            _contactListRepository, 
            _contactRepository, 
            _mediator, 
            _logger);
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_ValidInput_ReturnsSuccess()
    {
        ContactList contactList = CreateTestContactList();
        var command = new RemoveContactFromContactListCommand(contactList, "existing@example.com", true);

        _contactListRepository
            .UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(contactList));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(contactList.Contacts, Has.None.Matches<Recipient>(r => r.EmailAddress == "existing@example.com"));
        });
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_ContactNotInList_ReturnsFailure()
    {
        ContactList contactList = CreateTestContactList();
        var command = new RemoveContactFromContactListCommand(contactList, "notfound@example.com", true);

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.ContactDoesNotExistInContactList));
        });
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_UpdateFails_ReturnsFailure()
    {
        ContactList contactList = CreateTestContactList();
        var command = new RemoveContactFromContactListCommand(contactList, "existing@example.com", true);

        _contactListRepository
            .UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_NotAdmin_SendsNotificationEmail()
    {
        ContactList contactList = CreateTestContactList();
        var command = new RemoveContactFromContactListCommand(contactList, "existing@example.com", false);

        _contactListRepository
            .UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(contactList));

        _contactRepository
            .FindAsync(Arg.Any<Expression<Func<Recipient, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new List<Recipient>()));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<SendEmailCommand>(cmd =>
                cmd.Email.SenderEmailAddress == ValidSenderEmailAddresses.Auto &&
                cmd.Email.Recipients.Any(r => r.EmailAddress == "existing@example.com")),
            Arg.Any<CancellationToken>());

        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_ExceptionThrown_ReturnsFailure()
    {
        ContactList contactList = CreateTestContactList();
        var command = new RemoveContactFromContactListCommand(contactList, "existing@example.com", true);

        _contactListRepository
            .When(repo => repo.UpdateAsync(Arg.Any<ContactList>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("Simulated exception"));

        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }
    
    private static ContactList CreateTestContactList()
    {
        return new ContactList
        {
            Name = "Test ContactList",
            Description = "A test contact list",
            Contacts = [new Recipient() { EmailAddress = "existing@example.com" }]
        };
    }
}
