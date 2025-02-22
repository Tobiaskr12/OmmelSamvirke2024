using AutoFixture;
using Contracts.ServiceModules.Emails.ContactLists;
using FluentResults;
using DomainModules.Emails.Entities;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Emails.ContactLists.Queries;

[TestFixture, Category("IntegrationTests")]
public class CountContactsInContactListQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_WhenContactListExists_ReturnsContactCount()
    {
        var contactList = GlobalTestSetup.Fixture.Create<ContactList>();
        await AddTestData(contactList);
        
        var query = new CountContactsInContactListQuery(contactList.Id);
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(contactList.Contacts.Count));
        });
    }

    [Test]
    public async Task Handle_WhenContactListDoesNotExist_ReturnsFailure()
    {
        var query = new CountContactsInContactListQuery(99);
        
        Result<int> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors.First().Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }
}
