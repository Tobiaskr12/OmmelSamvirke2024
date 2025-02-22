using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetContactListQueryTests : ServiceTestBase
{
    [Test]
    public async Task GetContactList_ValidId_ReturnsSuccess()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        await AddTestData(contactList);
        
        var query = new GetContactListQuery(contactList.Id);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Id, Is.EqualTo(contactList.Id));
        });
    }

    [Test]
    public async Task GetContactList_IdNotFound_ReturnsFailure()
    {
        var query = new GetContactListQuery(99);
        Result<ContactList> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.That(result.IsFailed);
    }
}
