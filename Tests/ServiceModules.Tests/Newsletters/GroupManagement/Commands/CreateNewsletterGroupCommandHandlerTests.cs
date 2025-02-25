using AutoFixture;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.GroupManagement.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateNewsletterGroupCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task WhenCreatingNewsletterGroup_TheGroupIsReturnedWithAnId()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();

        var command = new CreateNewsletterGroupCommand(newsletterGroup);
        Result<NewsletterGroup> result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Name, Is.EqualTo(newsletterGroup.Name));
            Assert.That(result.Value.ContactList.Id, Is.EqualTo(newsletterGroup.ContactList.Id));
        });
    }
}
