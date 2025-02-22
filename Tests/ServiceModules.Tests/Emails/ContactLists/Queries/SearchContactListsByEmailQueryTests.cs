using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("IntegrationTests")]
public class SearchContactListsByEmailQueryTests : ServiceTestBase
{
    [Test]
    public async Task Handle_WhenContactListsExist_ReturnsContactLists()
    {
        var contactList1 = GlobalTestSetup.Fixture.Create<ContactList>();
        var contactList2 = GlobalTestSetup.Fixture.Create<ContactList>();
        contactList2.Contacts.Add(contactList1.Contacts.First());
        await AddTestData([contactList1, contactList2]);
        
        var query = new SearchContactListsByEmailQuery(contactList1.Contacts.First().EmailAddress);
        Result<List<ContactList>> result = await GlobalTestSetup.Mediator.Send(query);

        int[] expectedIds = [contactList1.Id, contactList2.Id];
        IEnumerable<int> actualIds = result.Value.Select(cl => cl.Id);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(expectedIds, Is.EquivalentTo(actualIds));
        });
    }

    [Test]
    public async Task Handle_WhenNoContactListsExist_ReturnsEmptyList()
    {
        const string emailAddress = "nonexistent@example.com";
        
        var query = new SearchContactListsByEmailQuery(emailAddress);
        Result<List<ContactList>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
