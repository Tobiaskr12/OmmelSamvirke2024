using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateContactListCommandTests : ServiceTestBase
{
    [Test]
    public async Task CreateContactListCommand_ValidInput_ReturnsSuccess()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        
        var command = new CreateContactListCommand(contactList);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task CreateContactListCommand_DuplicateRecipients_ReplacesContactsAndReturnsSuccess()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        var duplicateRecipientOne = GlobalTestSetup.Fixture.Create<Recipient>();
        var duplicateRecipientTwo = GlobalTestSetup.Fixture.Create<Recipient>();
        var uniqueRecipient = GlobalTestSetup.Fixture.Create<Recipient>();
        
        duplicateRecipientOne.EmailAddress = duplicateRecipientTwo.EmailAddress;
        contactList.Contacts = [duplicateRecipientOne, uniqueRecipient];
        await AddTestData(duplicateRecipientOne);

        var command = new CreateContactListCommand(contactList);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Contacts.Count(x => x.EmailAddress == duplicateRecipientOne.EmailAddress), Is.EqualTo(1));
        });
    }
}
