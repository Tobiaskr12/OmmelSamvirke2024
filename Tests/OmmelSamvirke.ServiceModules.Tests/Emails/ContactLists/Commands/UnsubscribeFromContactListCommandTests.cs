using System.Linq.Expressions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class UnsubscribeFromContactListCommandTests
{
    private ILogger _logger;
    private IMediator _mediator;
    private IRepository<ContactList> _repository;
    private IEmailTemplateEngine _emailTemplateEngine;
    private UnsubscribeFromContactListCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger>();
        _mediator = Substitute.For<IMediator>();
        _repository = Substitute.For<IRepository<ContactList>>();
        _emailTemplateEngine = Substitute.For<IEmailTemplateEngine>();
        _handler = new UnsubscribeFromContactListCommandHandler(_logger, _mediator, _emailTemplateEngine, _repository);
    }

    private void ConfigureFindAsync(Result<List<ContactList>> result) =>
        _repository.FindAsync(
            Arg.Any<Expression<Func<ContactList, bool>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(result));

    private void ConfigureUpdateAsync(ContactList expectedContactList, Result<ContactList> result) =>
        _repository.UpdateAsync(
            expectedContactList,
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(result));

    [Test]
    public async Task UnsubscribeFromContactList_FailedFindQuery_ReturnsFailure()
    {
        var command = new UnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid());
        ConfigureFindAsync(Result.Fail<List<ContactList>>("Query failed"));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task UnsubscribeFromContactList_EmptyFindQuery_ReturnsFailure()
    {
        var command = new UnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid());
        ConfigureFindAsync(Result.Ok(new List<ContactList>()));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    /// <summary>
    /// When a single contact list is returned and the recipient is present,
    /// removal succeeds, an email receipt is sent, and the command returns a success response.
    /// </summary>
    [Test]
    public async Task UnsubscribeFromContactList_SingleContactList_RemovesRecipientAndReturnsSuccess()
    {
        var recipient = new Recipient { EmailAddress = "test@example.com" };
        var otherRecipient = new Recipient { EmailAddress = "other@example.com" };

        var contactList = new ContactList
        {
            Name = "List1",
            Description = "First list",
            Contacts = [recipient, otherRecipient]
        };
        Guid token = contactList.UnsubscribeToken;
        var command = new UnsubscribeFromContactListCommand("test@example.com", token);

        // Simulate an update where the recipient is successfully removed.
        var updatedContactList = new ContactList
        {
            Name = contactList.Name,
            Description = contactList.Description,
            Contacts = [otherRecipient]
        };

        // Configure repository calls.
        ConfigureFindAsync(Result.Ok(new List<ContactList> { contactList }));
        ConfigureUpdateAsync(Arg.Any<ContactList>(), Result.Ok(updatedContactList));

        // Configure email template engine.
        _emailTemplateEngine.GenerateBodiesFromTemplate("UnsubscribeReceipt.html").Returns(Result.Ok());
        _emailTemplateEngine.GetSubject().Returns("Unsubscribe Receipt");
        _emailTemplateEngine.GetHtmlBody().Returns("<html>You have been unsubscribed.</html>");
        _emailTemplateEngine.GetPlainTextBody().Returns("You have been unsubscribed.");

        // Configure mediator to return a successful result when sending the email.
        _mediator.Send(
            Arg.Any<SendEmailCommand>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(Result.Ok(new EmailSendingStatus(null!, SendingStatus.Succeeded, []))));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess);
    }

    /// <summary>
    /// If the update returns success but the recipient still exists in the contact list,
    /// the command returns a failure.
    /// </summary>
    [Test]
    public async Task UnsubscribeFromContactList_UpdateSucceedsButRecipientStillPresent_ReturnsFailure()
    {
        var recipient = new Recipient { EmailAddress = "test@example.com" };
        var contactList = new ContactList
        {
            Name = "List1",
            Description = "First list",
            Contacts = [recipient]
        };
        Guid token = contactList.UnsubscribeToken;
        var command = new UnsubscribeFromContactListCommand("test@example.com", token);

        ConfigureFindAsync(Result.Ok(new List<ContactList> { contactList }));

        // Simulate that the recipient was not removed.
        var copyOfContactList = new ContactList
        {
            Name = contactList.Name,
            Description = contactList.Description,
            Contacts = [..contactList.Contacts]
        };
        ConfigureUpdateAsync(contactList, Result.Ok(copyOfContactList));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task UnsubscribeFromContactList_UpdateFails_ReturnsFailure()
    {
        var recipient = new Recipient { EmailAddress = "test@example.com" };
        var contactList = new ContactList
        {
            Name = "List1",
            Description = "First list",
            Contacts = [recipient]
        };
        Guid token = contactList.UnsubscribeToken;
        var command = new UnsubscribeFromContactListCommand("test@example.com", token);

        ConfigureFindAsync(Result.Ok(new List<ContactList> { contactList }));
        ConfigureUpdateAsync(contactList, Result.Fail<ContactList>("Update failed"));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task UnsubscribeFromContactList_ExceptionThrown_ReturnsFailure()
    {
        var command = new UnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid());
        _repository.FindAsync(
            Arg.Any<Expression<Func<ContactList, bool>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns<Task<Result<List<ContactList>>>>(_ => throw new Exception("Simulated exception"));

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
