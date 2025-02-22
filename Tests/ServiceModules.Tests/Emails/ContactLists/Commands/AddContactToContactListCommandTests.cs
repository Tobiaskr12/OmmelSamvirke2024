using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("IntegrationTests")]
public class AddContactToContactListCommandTests : ServiceTestBase
{
    [Test]
    public async Task AddContactToContactListCommand_ValidInput_ReturnsSuccess()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        await AddTestData(contactList);
        
        var command = new AddContactToContactListCommand(contactList, recipient);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(contactList.Contacts, Contains.Item(recipient));
        });
    }
    
    [Test]
    public async Task AddContactToContactListCommand_DuplicateRecipient_ReturnsFail()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        contactList.Contacts.Clear();
        contactList.Contacts.Add(recipient);
        await AddTestData(contactList);
        
        var command = new AddContactToContactListCommand(contactList, recipient);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(contactList.Contacts.First().Id, Is.EqualTo(recipient.Id));
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.ContactList_AddContact_ContactAlreadyExitsts));
        });
    }
}
