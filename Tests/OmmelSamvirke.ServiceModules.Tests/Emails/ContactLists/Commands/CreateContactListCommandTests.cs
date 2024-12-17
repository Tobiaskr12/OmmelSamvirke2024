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
}
