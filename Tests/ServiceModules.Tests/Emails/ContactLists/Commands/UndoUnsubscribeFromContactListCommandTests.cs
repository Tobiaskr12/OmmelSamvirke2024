using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("IntegrationTests")]
public class UndoUnsubscribeFromContactListCommandTests : ServiceTestBase
{
    [Test]
    public async Task UndoUnsubscribe_RecordNotFound_ReturnsFailure()
    {
        var command = new UndoUnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid(), Guid.NewGuid());
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_RecordExpired_ReturnsFailure()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        await AddTestData(contactList);
        
        var unsubscription = GlobalTestSetup.Fixture.Create<ContactListUnsubscription>();
        unsubscription.DateCreated = DateTime.UtcNow.AddDays(-15);
        unsubscription.ContactListId = contactList.Id;
        await AddTestData(unsubscription);

        var command = new UndoUnsubscribeFromContactListCommand(unsubscription.EmailAddress, contactList.UnsubscribeToken, unsubscription.UndoToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_ContactListNotFound_ReturnsFailure()
    {
        var unsubscription = GlobalTestSetup.Fixture.Create<ContactListUnsubscription>();
        unsubscription.DateCreated = DateTime.UtcNow.AddDays(-2);
        await AddTestData(unsubscription);
        
        var command = new UndoUnsubscribeFromContactListCommand(unsubscription.EmailAddress, Guid.NewGuid(), unsubscription.UndoToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UndoUnsubscribe_Success_AddsRecipientAndReturnsSuccess()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        await AddTestData(contactList);
        
        var unsubscription = GlobalTestSetup.Fixture.Create<ContactListUnsubscription>();
        unsubscription.DateCreated = DateTime.UtcNow.AddDays(-2);
        unsubscription.ContactListId = contactList.Id;
        await AddTestData(unsubscription);
        
        var command = new UndoUnsubscribeFromContactListCommand(unsubscription.EmailAddress, contactList.UnsubscribeToken, unsubscription.UndoToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsSuccess);
    }
}
