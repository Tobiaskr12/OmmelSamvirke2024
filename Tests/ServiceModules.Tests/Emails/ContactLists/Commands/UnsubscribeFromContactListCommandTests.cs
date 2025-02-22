using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;
using MimeKit;

namespace ServiceModules.Tests.Emails.ContactLists.Commands;

[TestFixture, Category("IntegrationTests")]
public class UnsubscribeFromContactListCommandTests : ServiceTestBase
{
    [Test]
    public async Task UnsubscribeFromContactList_FailedFindQuery_ReturnsFailure()
    {
        var command = new UnsubscribeFromContactListCommand("test@example.com", Guid.NewGuid());
        Result result = await GlobalTestSetup.Mediator.Send(command);

        Assert.That(result.IsFailed);
    }
    
    [Test]
    public async Task UnsubscribeFromContactList_SingleContactList_RemovesRecipientAndReturnsSuccess()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        recipient.EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        var otherRecipient = GlobalTestSetup.Fixture.Create<Recipient>();
        await AddTestData([recipient, otherRecipient]);
        
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        contactList.Contacts.Clear();
        contactList.Contacts.AddRange([recipient, otherRecipient]);
        await AddTestData(contactList);
        
        var command = new UnsubscribeFromContactListCommand(recipient.EmailAddress, contactList.UnsubscribeToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "Bekræft at du ønsker at blive afmeldt kontaktlisten");
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(contactList.Contacts, Has.Count.EqualTo(1));
            Assert.That(contactList.Contacts.First().Id, Is.EqualTo(otherRecipient.Id));
            Assert.That(receivedEmail, Is.Not.Null);
        });
    }
}
