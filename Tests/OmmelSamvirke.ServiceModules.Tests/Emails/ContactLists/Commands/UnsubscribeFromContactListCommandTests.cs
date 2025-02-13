using FluentResults;
using MediatR;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Interfaces.Emails;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using TestDatabaseFixtures;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class UnsubscribeFromContactListCommandTests
{
    private ILoggingHandler _logger;
    private IMediator _mediator;
    private IRepository<ContactList> _contactListRepository;
    private IRepository<ContactListUnsubscription> _contactListUnsubscriptionRepository;
    private IEmailTemplateEngine _emailTemplateEngine;
    private UnsubscribeFromContactListCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILoggingHandler>();
        _mediator = Substitute.For<IMediator>();
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _contactListUnsubscriptionRepository = Substitute.For<IRepository<ContactListUnsubscription>>();
        _emailTemplateEngine = Substitute.For<IEmailTemplateEngine>();
        _handler = new UnsubscribeFromContactListCommandHandler(
            _logger,
            _mediator,
            _emailTemplateEngine,
            _contactListRepository,
            _contactListUnsubscriptionRepository
        );

        // Return a valid unsubscription record with required properties.
        _contactListUnsubscriptionRepository.AddAsync(
            Arg.Any<ContactListUnsubscription>(),
            Arg.Any<CancellationToken>()
        ).Returns(MockHelpers.SuccessAsyncResult(new ContactListUnsubscription
        {
            EmailAddress = "test@example.com",
            ContactListId = 1
        }));
    }

    private void ConfigureFindAsync(Result<List<ContactList>> result) =>
        _contactListRepository.FindAsync(default!).ReturnsForAnyArgs(Task.FromResult(result));

    private void ConfigureUpdateAsync(ContactList expectedContactList, Result<ContactList> result) =>
        _contactListRepository.UpdateAsync(
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
            Id = 1,
            Name = "List1",
            Description = "First list",
            Contacts = [recipient, otherRecipient]
        };
        Guid token = contactList.UnsubscribeToken;
        var command = new UnsubscribeFromContactListCommand("test@example.com", token);

        // Simulate an update where the recipient is successfully removed.
        var updatedContactList = new ContactList
        {
            Id = 1,
            Name = contactList.Name,
            Description = contactList.Description,
            Contacts = [otherRecipient]
        };

        // Configure repository calls.
        ConfigureFindAsync(Result.Ok(new List<ContactList> { contactList }));
        ConfigureUpdateAsync(Arg.Any<ContactList>(), Result.Ok(updatedContactList));

        // Configure email template engine.
        _emailTemplateEngine.GenerateBodiesFromTemplate("Empty.html", Arg.Any<(string key, string value)[]>()).Returns(Result.Ok());
        _emailTemplateEngine.GetSubject().Returns("Unsubscribe Receipt");
        _emailTemplateEngine.GetHtmlBody().Returns("<html>You have been unsubscribed.</html>");
        _emailTemplateEngine.GetPlainTextBody().Returns("You have been unsubscribed.");

        // Configure mediator to return a successful result when sending the email.
        _mediator.Send(
            Arg.Any<SendEmailCommand>(),
            Arg.Any<CancellationToken>()
        ).Returns(MockHelpers.SuccessAsyncResult(new EmailSendingStatus(null!, SendingStatus.Succeeded, [])));

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
            Id = 1,
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
            Id = 1,
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
            Id = 1,
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
        _contactListRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs<Task<Result<List<ContactList>>>>(_ => throw new Exception("Simulated exception"));

        Result result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsFailed);
    }
}

[TestFixture, Category("IntegrationTests")]
public class UnsubscribeFromContactListIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;

    [OneTimeSetUp]
    public void OneTimeSetUp() =>
        _integrationTestingHelper = new IntegrationTestingHelper();

    [SetUp]
    public async Task Setup() =>
        await _integrationTestingHelper.ResetDatabase();

    [Test]
    public async Task UnsubscribeFromContactList_HappyPath_ReturnsSuccessAndCreatesUnsubscriptionRecord()
    {
        const string email = "ommelsamvirketest1@gmail.com";
        var contactListRepository = _integrationTestingHelper.GetService<IRepository<ContactList>>();

        var contactList = new ContactList
        {
            Name = "Integration List",
            Description = "Integration test contact list",
            Contacts =
            [
                new Recipient { EmailAddress = email },
                new Recipient { EmailAddress = "ommelsamvirketest2@gmail.com" }
            ]
        };

        Result<ContactList> addResult = await contactListRepository.AddAsync(contactList);
        Assert.That(addResult.IsSuccess, Is.True);

        var command = new UnsubscribeFromContactListCommand(email, addResult.Value.UnsubscribeToken);
        Result result = await _integrationTestingHelper.Mediator.Send(command);
        Assert.That(result.IsSuccess, Is.True);

        Result<ContactList> fetchedContactList = await contactListRepository.GetByIdAsync(addResult.Value.Id);
        Assert.Multiple(() =>
        {
            Assert.That(fetchedContactList.IsSuccess, Is.True);
            Assert.That(fetchedContactList.Value.Contacts.Exists(r => r.EmailAddress == email), Is.False);
        });

        var unsubscriptionRepository = _integrationTestingHelper.GetService<IRepository<ContactListUnsubscription>>();

        Result<List<ContactListUnsubscription>> allUnsubscriptions = await unsubscriptionRepository.GetAllAsync();
        Assert.Multiple(() =>
        {
            Assert.That(allUnsubscriptions.IsSuccess, Is.True);
            Assert.That(allUnsubscriptions.Value.Exists(u => u.EmailAddress == email && u.ContactListId == addResult.Value.Id), Is.True);
        });
    }
}
