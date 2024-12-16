using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("UnitTests")]
public class CreateContactListCommandTests
{
    private ILogger _logger;
    private IRepository<ContactList> _repository;
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
        
        _handler = new CreateContactListCommandHandler(_repository, _logger);
    }

    [Test]
    public async Task CreateContactListCommand_ValidInput_ReturnsSuccess()
    {
        var command = new CreateContactListCommand(_baseValidContactList);
        _repository.AddAsync(_baseValidContactList).Returns(_baseValidContactList);
        
        Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);
        
        Assert.That(result.IsSuccess);
    }
    
    // TODO - Implement when it can be called via MediatR
    // [Test]
    // public async Task CreateContactListCommand_SomeValidContacts_ReturnsFail()
    // {
    //     _baseValidContactList.Contacts.Add(CreateRecipient("valid1@example.com"));
    //     _baseValidContactList.Contacts.Add(CreateRecipient("valid2@example.com"));
    //     _baseValidContactList.Contacts.Add(CreateRecipient("invalid3.com"));
    //     
    //     var command = new CreateContactListCommand(_baseValidContactList);
    //     _repository.AddAsync(_baseValidContactList).Returns(_baseValidContactList);
    //     
    //     Result<ContactList> result = await _handler.Handle(command, CancellationToken.None);
    //     
    //     Assert.That(result.IsFailed);
    // }

    // private static Recipient CreateRecipient(string email)
    // {
    //     return new Recipient
    //     {
    //         EmailAddress = email,
    //     };
    // }
}