using AutoFixture;
using FluentResults;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;
using Contracts.ServiceModules.Emails.ContactLists;
using MimeKit;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("IntegrationTests")]
public class RemoveContactFromContactListCommandTests : ServiceTestBase
{
    [Test]
    public async Task RemoveContactFromContactListCommand_ValidInput_ReturnsSuccess()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        Recipient firstRecipient = contactList.Contacts.First();
        await AddTestData(contactList);
        
        var command = new RemoveContactFromContactListCommand(contactList, firstRecipient.EmailAddress, true);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(contactList.Contacts, Has.None.Matches<Recipient>(r => r.EmailAddress ==  firstRecipient.EmailAddress));
        });
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_ContactNotInList_ReturnsFailure()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        await AddTestData(contactList);
        
        var command = new RemoveContactFromContactListCommand(contactList, "notfound@example.com", true);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.ContactDoesNotExistInContactList));
        });
    }

    [Test]
    public async Task RemoveContactFromContactListCommand_NotAdmin_SendsNotificationEmail()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        contactList.Contacts.First().EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        await AddTestData(contactList);
        
        var command = new RemoveContactFromContactListCommand(contactList, GlobalTestSetup.TestEmailClientOne.EmailAddress, false);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(command);

        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "Vi har afmeldt dig kontaktlisten");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(receivedEmail, Is.Not.Null);
        });
    }
}
